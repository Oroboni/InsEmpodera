#!/usr/bin/env bash
# Atualiza o EcoRedes na EC2 com a versão da main e recria só o que mudou (os volumes do banco ficam).
# Usado pelo deploy automático (GitHub Actions) e também à mão: cd /opt/ecoredes && ./deploy/aws/atualizar.sh
set -euo pipefail
cd /opt/ecoredes
git fetch --quiet origin main
git reset --quiet --hard origin/main
echo "Versão: $(git log --oneline -1)"
docker compose -f compose.yaml -f compose.aws.yaml up -d --build --remove-orphans
docker image prune -f >/dev/null

# Espera a aplicação responder (até ~3 min: a primeira subida cria o banco).
# Passa pelo Nginx (que informa HTTPS) com o domínio aceito pela aplicação (APP_DOMINIO do .env).
dominio=$(grep -E '^APP_DOMINIO=' .env | cut -d= -f2- | cut -d';' -f1 | tr -d '"' || true)
[ -z "$dominio" ] || [ "$dominio" = "*" ] && dominio=localhost
for i in $(seq 1 36); do
  codigo=$(curl -s -o /dev/null -w '%{http_code}' -H "Host: $dominio" http://127.0.0.1/Account || true)
  if [ "$codigo" = "200" ]; then echo "Aplicação no ar (HTTP 200)."; exit 0; fi
  sleep 5
done
echo "A aplicação não respondeu. Veja: docker compose -f compose.yaml -f compose.aws.yaml logs --tail 80 app" >&2
exit 1
