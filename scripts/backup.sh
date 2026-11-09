#!/usr/bin/env bash
# Backup do EcoRedes rodando no Docker: banco (mysqldump) + fotos e cores (App_Data).
# Uso: scripts/backup.sh [pasta-destino]   (padrão: backups/, guardados por DIAS_RETENCAO dias)
set -euo pipefail
cd "$(dirname "$0")/.."
DESTINO="${1:-backups}"
DIAS_RETENCAO="${DIAS_RETENCAO:-14}"
MOMENTO="$(date +%F-%H%M%S)"
mkdir -p "$DESTINO"

BANCO="$DESTINO/banco-$MOMENTO.sql.gz"
ARQUIVOS="$DESTINO/app-data-$MOMENTO.tar.gz"

docker compose exec -T mysql sh -c 'exec mysqldump -uroot -p"$MYSQL_ROOT_PASSWORD" --single-transaction --routines --triggers --default-character-set=utf8mb4 --databases empodera 2>/dev/null' | gzip > "$BANCO"
docker compose exec -T app tar czf - -C /app App_Data > "$ARQUIVOS"

# Um backup só vale se puder ser lido: confere os dois arquivos.
gzip -t "$BANCO" && gunzip -c "$BANCO" | grep -q "CREATE TABLE \`usuarios\`" || { echo "ERRO: backup do banco inválido: $BANCO" >&2; exit 1; }
tar tzf "$ARQUIVOS" > /dev/null || { echo "ERRO: backup dos arquivos inválido: $ARQUIVOS" >&2; exit 1; }

find "$DESTINO" -name 'banco-*.sql.gz' -mtime +"$DIAS_RETENCAO" -delete
find "$DESTINO" -name 'app-data-*.tar.gz' -mtime +"$DIAS_RETENCAO" -delete
echo "Backup concluído:"
ls -lh "$BANCO" "$ARQUIVOS"
