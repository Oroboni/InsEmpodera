#!/usr/bin/env bash
# Liga o deploy automático (timer de 5 minutos) neste servidor, para o usuário atual.
# Uso: cd /opt/ecoredes && ./deploy/servidor/instalar-deploy-automatico.sh
set -euo pipefail
sudo tee /etc/systemd/system/ecoredes-deploy.service >/dev/null <<UNIT
[Unit]
Description=EcoRedes: publica a main quando o CI passa
Wants=network-online.target docker.service
After=network-online.target docker.service

[Service]
Type=oneshot
User=$USER
ExecStart=/opt/ecoredes/deploy/servidor/deploy-automatico.sh
TimeoutStartSec=1800
UNIT
sudo tee /etc/systemd/system/ecoredes-deploy.timer >/dev/null <<'UNIT'
[Unit]
Description=EcoRedes: confere a main a cada 5 minutos

[Timer]
OnBootSec=2min
OnUnitActiveSec=5min
Persistent=true

[Install]
WantedBy=timers.target
UNIT
sudo systemctl daemon-reload
sudo systemctl enable --now ecoredes-deploy.timer
systemctl list-timers ecoredes-deploy.timer --no-pager | head -2
echo "Histórico: journalctl -u ecoredes-deploy -n 30"
