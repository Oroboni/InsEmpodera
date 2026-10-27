#!/usr/bin/env bash
# Migra os dados do servidor antigo (chaves UUID) para o EcoRedes no Docker (MySQL 8.4) e gera um relatório.
#
# Uso: scripts/migrar-dados.sh <dump-do-banco-antigo.sql[.gz]> [app-data-antigo.tar.gz]
#
# Antes: o banco novo precisa ter sido criado pelo compose (docker compose up -d) e ainda não ter dados reais.
# O dump é o backup da base antiga: uma cópia dele fica em backups/ antes de qualquer alteração.
# Gere o dump com: mariadb-dump --single-transaction --hex-blob --default-character-set=utf8mb4 empodera | gzip > base-antiga.sql.gz
# Critérios de aceite (o script falha se algum não for atendido):
#   1. a verificação da migração não encontra problemas (órfãos, nulos, tamanhos, domínios, duplicidades);
#   2. cada tabela tem no banco novo o mesmo número de registros do antigo (exceto vínculos repetidos, listados);
#   3. todas as chaves estrangeiras e verificações do esquema estão ativas (o banco recusa vínculos quebrados);
#   4. nenhuma menção a ator ficou com o UUID antigo.
set -euo pipefail
cd "$(dirname "$0")/.."
DUMP="${1:?informe o dump do banco antigo (.sql ou .sql.gz)}"
ARQUIVOS_ANTIGOS="${2:-}"
[ -f "$DUMP" ] || { echo "Arquivo não encontrado: $DUMP" >&2; exit 1; }
MOMENTO="$(date +%F-%H%M%S)"
mkdir -p backups
RELATORIO="backups/relatorio-migracao-$MOMENTO.txt"

# SQL sempre pela entrada padrão (sem aspas aninhadas); o argumento opcional é o banco padrão.
root_sql() { docker compose exec -T mysql sh -c 'mysql -uroot -p"$MYSQL_ROOT_PASSWORD" --default-character-set=utf8mb4 -N -B '"${1:-}"' 2>/dev/null'; }
consulta() { printf '%s\n' "$1" | root_sql; }
ler_dump() { case "$DUMP" in *.gz) gunzip -c "$DUMP" ;; *) cat "$DUMP" ;; esac; }

exec > >(tee "$RELATORIO") 2>&1
echo "== Migração EcoRedes ($MOMENTO)"
echo "Dump: $DUMP"

echo "== 1. Backup da base antiga"
cp "$DUMP" "backups/base-antiga-$MOMENTO-$(basename "$DUMP")"
shasum -a 256 "$DUMP" | sed 's/^/sha256 /'

echo "== 2. Parando a aplicação"
docker compose stop app

echo "== 3. Importando a base antiga em empodera_antigo"
if [ "$(consulta "SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME='empodera_antigo'")" != "0" ]; then
  echo "ERRO: empodera_antigo já existe. Apague-o (DROP DATABASE empodera_antigo) se for de uma tentativa anterior." >&2
  exit 1
fi
consulta "CREATE DATABASE empodera_antigo"
# Dumps feitos com --databases trazem CREATE DATABASE/USE do banco original: são ignorados para tudo cair em empodera_antigo.
# Funções do MariaDB em valores padrão de colunas são trocadas pelas do MySQL (a migração só lê os dados).
ler_dump \
  | LC_ALL=C sed -E '/^(CREATE DATABASE|USE |drop DATABASE|DROP DATABASE)/Id; /^[[:space:]]*(use|create database|drop database) /Id' \
  | LC_ALL=C sed "s/UNHEX(REPLACE(UUID_v4(), '-', ''))/UUID_TO_BIN(UUID())/g" \
  | root_sql empodera_antigo
echo "Tabelas importadas: $(consulta "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA='empodera_antigo'")"

echo "== 4. Migrando (verifica antes; grava tudo ou nada)"
if ! root_sql < Database/migracao-uuid-para-int.sql > "backups/saida-migracao-$MOMENTO.txt"; then
  cat "backups/saida-migracao-$MOMENTO.txt"
  echo "ERRO: a migração parou sem gravar nada. Corrija os problemas listados em empodera_antigo e rode de novo." >&2
  exit 1
fi

echo "== 5. Comparando as tabelas (antigo -> novo)"
falhas=0
for tabela in $(consulta "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA='empodera' ORDER BY TABLE_NAME"); do
  antigo=$(consulta "SELECT COUNT(*) FROM empodera_antigo.\`$tabela\`" 2>/dev/null || echo "?")
  novo=$(consulta "SELECT COUNT(*) FROM empodera.\`$tabela\`")
  situacao="ok"
  if [ "$antigo" != "$novo" ]; then
    case "$tabela" in
      eixos|atividadeseixo|diarioeixos|redeeixos|vulnerabilidadeseixo|detalheseixos|diariosprocessoeixos|acoesatores|daatores|diarioacoes|permissoes|redesprimarias)
        situacao="repetidos removidos (UNIQUE)";;
      *) situacao="DIFERENTE"; falhas=1;;
    esac
  fi
  printf "  %-24s %6s -> %-6s %s\n" "$tabela" "$antigo" "$novo" "$situacao"
done

echo "== 6. Integridade"
echo "  Restrições ativas: $(consulta "SELECT GROUP_CONCAT(CONCAT(CONSTRAINT_TYPE,'=',n) SEPARATOR ', ') FROM (SELECT CONSTRAINT_TYPE, COUNT(*) n FROM information_schema.TABLE_CONSTRAINTS WHERE TABLE_SCHEMA='empodera' GROUP BY CONSTRAINT_TYPE) t")"
uuids=$(consulta "SELECT COUNT(*) FROM empodera.diariosprocessopessoal WHERE Descricao LIKE '%(ator:________-____-%'")
echo "  Menções ainda com UUID antigo: $uuids"
[ "$uuids" = "0" ] || falhas=1

if [ -n "$ARQUIVOS_ANTIGOS" ]; then
  echo "== 7. Fotos e cores (renomeadas do UUID para o novo Id)"
  mapa=$(consulta "SELECT CONCAT(REPLACE(BIN_TO_UUID(antigo), '-', ''), ' ', novo) FROM empodera_antigo.migracao_mapa_usuarios")
  docker compose run --rm -T --no-deps --entrypoint sh -e MAPA="$mapa" app -c '
    set -e; mkdir -p /tmp/antigo && tar xzf - -C /tmp/antigo --warning=no-unknown-keyword
    origem=$(find /tmp/antigo -type d -name App_Data | head -1); [ -n "$origem" ] || origem=/tmp/antigo
    mkdir -p /app/App_Data/fotos-usuarios /app/App_Data/temas-usuarios
    echo "$MAPA" | while read -r antigo novo; do
      if [ -z "$antigo" ]; then continue; fi
      for f in "$origem"/fotos-usuarios/"$antigo".*; do
        if [ -e "$f" ]; then cp "$f" "/app/App_Data/fotos-usuarios/$novo.${f##*.}"; echo "  foto $antigo -> $novo"; fi
      done
      if [ -e "$origem/temas-usuarios/$antigo.json" ]; then
        cp "$origem/temas-usuarios/$antigo.json" "/app/App_Data/temas-usuarios/$novo.json"; echo "  cores $antigo -> $novo"
      fi
    done' < "$ARQUIVOS_ANTIGOS"
fi

echo "== 8. Iniciando a aplicação"
docker compose start app

if [ "$falhas" = "0" ]; then
  echo "RESULTADO: migração aprovada nos critérios automáticos. Faça a conferência manual do roteiro (docs/docker.md)."
  echo "Depois de validar o sistema, apague a cópia antiga: DROP DATABASE empodera_antigo (contém dados pessoais)."
else
  echo "RESULTADO: há diferenças acima. Não coloque em produção antes de entender cada uma." >&2
  exit 1
fi
