#!/usr/bin/env bash
# Remove do HISTÓRICO do Git os arquivos com dados pessoais e hashes de senha (ver docs/seguranca-dados.md).
# Trabalha numa cópia espelho em outra pasta: o repositório local da equipe não é tocado.
# Só envia ao GitHub com --enviar, e isso deve ser feito pelo dono do repositório, combinado com a equipe
# (reescreve todos os commits; depois cada pessoa precisa clonar de novo).
#
# Requisitos: git e git-filter-repo (pip install git-filter-repo)
# Uso:  scripts/limpar-historico.sh                 -> limpa a cópia e mostra o relatório
#       scripts/limpar-historico.sh --enviar        -> depois de conferir, envia (force push)
set -euo pipefail
REMOTO="${REMOTO:-https://github.com/Oroboni/InsEmpodera.git}"
PASTA="${PASTA:-$HOME/InsEmpodera-limpeza-historico}"
command -v git-filter-repo >/dev/null || { echo "Instale o git-filter-repo: pip install git-filter-repo" >&2; exit 1; }

# Caminhos que nunca deveriam ter sido versionados (dados de comunidades, pessoas e hashes de senha).
CAMINHOS=(Banco.txt Empodera.db Empodera.testing.db deploy/ wwwroot/uploads/ strix_runs/)

if [[ "${1:-}" != "--enviar" ]]; then
  [ -e "$PASTA" ] && { echo "A pasta $PASTA já existe; apague-a ou defina PASTA=..." >&2; exit 1; }
  git clone --mirror "$REMOTO" "$PASTA"
  cd "$PASTA"
  echo "Antes:  $(git count-objects -vH | grep size-pack)"
  args=(); for c in "${CAMINHOS[@]}"; do args+=(--path "$c"); done
  git filter-repo --invert-paths "${args[@]}" --force
  echo "Depois: $(git count-objects -vH | grep size-pack)"
  echo "== Conferência (deve ficar vazio):"
  for c in "${CAMINHOS[@]}"; do git log --all --oneline -- "$c" | head -3; done
  echo "Cópia limpa em $PASTA. Revise e, com a equipe de acordo, rode: PASTA=$PASTA scripts/limpar-historico.sh --enviar"
else
  cd "$PASTA"
  git remote add origin "$REMOTO" 2>/dev/null || git remote set-url origin "$REMOTO"
  git push --force --mirror origin
  echo "Histórico reescrito no GitHub. Agora: cada pessoa clona de novo; peça ao suporte do GitHub a remoção"
  echo "das cópias em cache e das referências de pull requests (docs/seguranca-dados.md)."
fi
