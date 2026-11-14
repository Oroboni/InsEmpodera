#!/usr/bin/env bash
# Prepara uma EC2 Ubuntu 24.04 nova para o EcoRedes (rodar uma vez, como ubuntu):
#   curl -fsSL https://raw.githubusercontent.com/Oroboni/InsEmpodera/main/deploy/aws/preparar-ec2.sh | bash
set -euo pipefail
PASTA=/opt/ecoredes

echo "== Memória extra (swap de 2 GB) para compilar a imagem .NET"
if ! swapon --show | grep -q /swapfile; then
  sudo fallocate -l 2G /swapfile && sudo chmod 600 /swapfile
  sudo mkswap /swapfile && sudo swapon /swapfile
  echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab >/dev/null
fi

echo "== Fuso horário de Brasília"
sudo timedatectl set-timezone America/Sao_Paulo

echo "== Docker (com Compose e Buildx), Nginx e Git"
if ! command -v docker >/dev/null; then curl -fsSL https://get.docker.com | sudo sh; fi
sudo apt-get update -y && sudo apt-get install -y nginx git
sudo usermod -aG docker "$USER"
sudo systemctl enable --now docker nginx

echo "== Código do GitHub (branch main)"
if [ ! -d "$PASTA/.git" ]; then
  sudo git clone https://github.com/Oroboni/InsEmpodera.git "$PASTA"
fi
sudo chown -R "$USER:$USER" "$PASTA"

echo "== Nginx"
sudo cp "$PASTA/deploy/aws/nginx-ecoredes.conf" /etc/nginx/sites-available/ecoredes
sudo ln -sf /etc/nginx/sites-available/ecoredes /etc/nginx/sites-enabled/ecoredes
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t && sudo systemctl reload nginx

if [ ! -f "$PASTA/.env" ]; then
  cp "$PASTA/.env.example" "$PASTA/.env" && chmod 600 "$PASTA/.env"
  echo
  echo ">> Agora edite $PASTA/.env (nano $PASTA/.env), troque as senhas e rode:"
  echo ">>   cd $PASTA && ./deploy/aws/atualizar.sh"
fi
echo "== Pronto. Saia e entre de novo no terminal para usar docker sem sudo."
