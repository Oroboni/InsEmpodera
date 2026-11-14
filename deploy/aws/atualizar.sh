#!/usr/bin/env bash
# Mantido por compatibilidade: o script comum fica em deploy/servidor/ (o .env da AWS define COMPOSE_FILE).
exec /opt/ecoredes/deploy/servidor/atualizar.sh "$@"
