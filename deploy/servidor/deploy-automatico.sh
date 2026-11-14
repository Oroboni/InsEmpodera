#!/usr/bin/env bash
# CD por "pull" (sem segredos no GitHub): o servidor confere a main a cada 5 minutos (timer do systemd).
# Publica um commit novo só se o CI "Testes de backend e banco" dele terminou com sucesso, e:
#   - faz backup do banco e dos arquivos antes (scripts/backup.sh);
#   - não publica sozinho commit que traga script novo em Database/ (migração de banco é manual);
#   - se a versão nova não responder, volta para a anterior.
set -euo pipefail
principal() {
  cd /opt/ecoredes
  local repo=Oroboni/InsEmpodera atual novo resultado
  git fetch --quiet origin main
  atual=$(git rev-parse HEAD)
  novo=$(git rev-parse origin/main)
  [ "$atual" = "$novo" ] && return 0

  resultado=$(curl -fsS -H 'Accept: application/vnd.github+json' \
    "https://api.github.com/repos/$repo/actions/workflows/testes.yml/runs?branch=main&head_sha=$novo&per_page=1" \
    | python3 -c 'import json,sys; r=json.load(sys.stdin)["workflow_runs"]; print(r[0]["status"] + "/" + str(r[0]["conclusion"]) if r else "sem-ci")')

  case "$resultado" in
    completed/success) ;;
    completed/*) echo "WARN deploy: ${novo:0:7} falhou no CI ($resultado); versão atual mantida (${atual:0:7})"; return 0 ;;
    *) echo "INFO deploy: ${novo:0:7} aguardando CI ($resultado)"; return 0 ;;
  esac

  if git diff --name-only --diff-filter=A "$atual" "$novo" -- 'Database/*.sql' | grep -v 'Empodera.schema.sql' | grep -q .; then
    echo "WARN deploy: ${novo:0:7} traz migração de banco ($(git diff --name-only --diff-filter=A "$atual" "$novo" -- 'Database/*.sql' | tr '\n' ' ')); publique à mão depois de aplicá-la"
    return 0
  fi

  echo "INFO deploy: ${novo:0:7} passou no CI; backup antes de publicar"
  if [ -x scripts/backup.sh ] && docker compose ps --status running --services 2>/dev/null | grep -qx mysql; then
    scripts/backup.sh | tail -3 || { echo "ERROR deploy: backup falhou; nada foi publicado"; return 1; }
  fi

  if ./deploy/servidor/atualizar.sh; then
    echo "INFO deploy: ${novo:0:7} publicado"
  else
    echo "ERROR deploy: ${novo:0:7} não respondeu; voltando para ${atual:0:7}"
    git reset --quiet --hard "$atual"
    ./deploy/servidor/atualizar.sh --sem-pull && echo "INFO deploy: versão ${atual:0:7} restaurada"
    return 1
  fi
}
principal "$@"
