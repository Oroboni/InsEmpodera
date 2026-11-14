#!/usr/bin/env bash
# CD por "pull" (sem segredos no GitHub): a EC2 confere a main a cada 5 minutos (timer do systemd) e,
# se há commit novo cujo CI "Testes de backend e banco" terminou com sucesso, publica com atualizar.sh.
# Commit com testes falhando ou ainda rodando nunca é publicado.
set -euo pipefail
cd /opt/ecoredes
REPO=Oroboni/InsEmpodera

git fetch --quiet origin main
atual=$(git rev-parse HEAD)
novo=$(git rev-parse origin/main)
[ "$atual" = "$novo" ] && exit 0

resultado=$(curl -fsS -H 'Accept: application/vnd.github+json' \
  "https://api.github.com/repos/$REPO/actions/workflows/testes.yml/runs?branch=main&head_sha=$novo&per_page=1" \
  | python3 -c 'import json,sys; r=json.load(sys.stdin)["workflow_runs"]; print(r[0]["status"] + "/" + str(r[0]["conclusion"]) if r else "sem-ci")')

case "$resultado" in
  completed/success)
    echo "INFO deploy: ${novo:0:7} passou no CI, publicando"
    ./deploy/aws/atualizar.sh
    echo "INFO deploy: ${novo:0:7} publicado" ;;
  completed/*)
    echo "WARN deploy: ${novo:0:7} falhou no CI ($resultado); versão atual mantida (${atual:0:7})" ;;
  *)
    echo "INFO deploy: ${novo:0:7} aguardando CI ($resultado)" ;;
esac
