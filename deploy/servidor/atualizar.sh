#!/usr/bin/env bash
# Atualiza o EcoRedes com a versão da main e recria só o que mudou (os volumes do banco ficam).
# Serve para qualquer servidor (Hostinger, AWS): os arquivos do Compose vêm de COMPOSE_FILE no .env
# (ex.: AWS usa COMPOSE_FILE=compose.yaml:compose.aws.yaml para mandar os logs ao CloudWatch).
# Uso: cd /opt/ecoredes && ./deploy/servidor/atualizar.sh [--sem-pull]
set -euo pipefail
principal() {
  cd /opt/ecoredes
  if [ "${1:-}" != "--sem-pull" ]; then
    git fetch --quiet origin main
    git reset --quiet --hard origin/main
  fi
  echo "Versão: $(git log --oneline -1)"
  docker compose up -d --build --remove-orphans
  docker image prune -f >/dev/null

  # Fala direto com o contêiner, como o Nginx faria (HTTPS informado e domínio aceito: APP_DOMINIO do .env).
  local dominio porta codigo
  dominio=$(grep -E '^APP_DOMINIO=' .env | cut -d= -f2- | cut -d';' -f1 | tr -d '"' || true)
  { [ -z "$dominio" ] || [ "$dominio" = "*" ]; } && dominio=localhost
  porta=$(grep -E '^APP_PORTA=' .env | cut -d= -f2 | tr -d '"' || true)
  porta=${porta:-8080}
  for _ in $(seq 1 36); do
    codigo=$(curl -s -o /dev/null -w '%{http_code}' -H "Host: $dominio" -H 'X-Forwarded-Proto: https' \
      "http://127.0.0.1:$porta/Account" || true)
    if [ "$codigo" = "200" ]; then echo "Aplicação no ar (HTTP 200)."; return 0; fi
    sleep 5
  done
  echo "A aplicação não respondeu. Veja: docker compose logs --tail 80 app" >&2
  return 1
}
principal "$@"
