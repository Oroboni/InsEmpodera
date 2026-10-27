#!/usr/bin/env bash
# Restaura um backup feito por scripts/backup.sh. APAGA o banco atual e o substitui pelo do backup.
# Uso: scripts/restaurar.sh <banco-AAAA-MM-DD-HHMMSS.sql.gz> [app-data-AAAA-MM-DD-HHMMSS.tar.gz] [--sim]
set -euo pipefail
cd "$(dirname "$0")/.."
BANCO="${1:?informe o arquivo banco-*.sql.gz}"
ARQUIVOS="${2:-}"
[ "$ARQUIVOS" = "--sim" ] && ARQUIVOS=""
[ -f "$BANCO" ] || { echo "Arquivo não encontrado: $BANCO" >&2; exit 1; }
gzip -t "$BANCO"

if [[ " $* " != *" --sim "* ]]; then
  read -r -p "Isto substitui TODOS os dados atuais pelo backup $BANCO. Digite RESTAURAR para continuar: " resposta
  [ "$resposta" = "RESTAURAR" ] || { echo "Cancelado."; exit 1; }
fi

echo "Parando a aplicação..."
docker compose stop app
echo "Restaurando o banco..."
docker compose exec -T mysql sh -c 'mysql -uroot -p"$MYSQL_ROOT_PASSWORD" -e "DROP DATABASE IF EXISTS empodera" 2>/dev/null'
gunzip -c "$BANCO" | docker compose exec -T mysql sh -c 'mysql -uroot -p"$MYSQL_ROOT_PASSWORD" 2>/dev/null'

if [ -n "$ARQUIVOS" ]; then
  echo "Restaurando fotos e cores (App_Data)..."
  docker compose run --rm -T --no-deps --entrypoint sh app -c 'rm -rf /app/App_Data/* && tar xzf - -C /app' < "$ARQUIVOS"
fi

docker compose start app
echo "Restauração concluída. Confira o acesso ao sistema."
