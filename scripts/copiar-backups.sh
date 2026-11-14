#!/usr/bin/env bash
# Copia os backups do servidor (banco + fotos) para este computador: uma cópia fora da VPS.
# Uso: scripts/copiar-backups.sh <usuario@servidor> <chave-ssh> <pasta-destino> [dias-guardados]
# Ex.: scripts/copiar-backups.sh deploy@179.197.230.8 ~/.ssh/ecoredes_vps ~/Backups-EcoRedes 60
# Uma tentativa só, sem senha (BatchMode): nunca dispara o bloqueio do fail2ban.
set -euo pipefail
SERVIDOR="$1"; CHAVE="$2"; DESTINO="$3"; DIAS="${4:-60}"
mkdir -p "$DESTINO"
avisar() { command -v osascript >/dev/null && osascript -e "display notification \"$1\" with title \"Backup EcoRedes\"" || true; }

if ! rsync -a --timeout=60 -e "ssh -i $CHAVE -o BatchMode=yes -o IdentitiesOnly=yes -o ConnectTimeout=20" \
     --include='banco-*.sql.gz' --include='app-data-*.tar.gz' --exclude='*' \
     "$SERVIDOR:/opt/ecoredes/backups/" "$DESTINO/"; then
  avisar "Não consegui copiar os backups do servidor (internet ou firewall da VPS)."
  echo "$(date '+%F %T') ERRO ao copiar" >&2; exit 1
fi

# Confere o backup mais recente: um arquivo corrompido não serve para restaurar.
ultimo=$(ls -t "$DESTINO"/banco-*.sql.gz 2>/dev/null | head -1)
if [ -z "$ultimo" ] || ! gzip -t "$ultimo"; then
  avisar "O backup mais recente do banco veio corrompido ou não existe."
  echo "$(date '+%F %T') ERRO: backup inválido ($ultimo)" >&2; exit 1
fi
find "$DESTINO" \( -name 'banco-*.sql.gz' -o -name 'app-data-*.tar.gz' \) -mtime +"$DIAS" -delete
chmod 600 "$DESTINO"/* 2>/dev/null || true
echo "$(date '+%F %T') OK: $(ls "$DESTINO" | grep -c '^banco-') backups do banco; mais recente $(basename "$ultimo")"
