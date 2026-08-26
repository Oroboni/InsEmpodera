import { existsSync, readdirSync, readFileSync } from 'node:fs';
import { join, relative } from 'node:path';
import { describe, expect, it } from 'vitest';
import { projectRoot, readProjectFile } from './helpers/project.js';

function filesUnder(directory, extension) {
  const result = [];
  for (const entry of readdirSync(directory, { withFileTypes: true })) {
    const path = join(directory, entry.name);
    if (entry.isDirectory()) result.push(...filesUnder(path, extension));
    else if (!extension || entry.name.endsWith(extension)) result.push(path);
  }
  return result;
}

function normalizedProjectPath(path) {
  return relative(projectRoot, path).replaceAll('\\', '/');
}

const viewFiles = filesUnder(join(projectRoot, 'Views'), '.cshtml');
const firstPartyAssets = new Set(
  filesUnder(join(projectRoot, 'wwwroot')).map(path => relative(join(projectRoot, 'wwwroot'), path).replaceAll('\\', '/'))
);

function withoutRazorComments(source) {
  return source.replace(/@\*[\s\S]*?\*@/g, '');
}

function renderedIds(source) {
  const ids = new Set(
    [...source.matchAll(/\bid\s*=\s*["'](?<id>[^"'@<>]+)["']/gi)].map(match => match.groups.id)
  );
  for (const match of source.matchAll(/\basp-for\s*=\s*["'](?<property>[^"'@<>]+)["']/gi)) {
    ids.add(match.groups.property.split('.').at(-1));
  }
  return ids;
}

describe('integridade dos assets first-party', () => {
  it('toda referência direta a asset first-party existe com o mesmo case', () => {
    const missing = [];
    for (const viewFile of viewFiles) {
      const source = withoutRazorComments(readFileSync(viewFile, 'utf8'));
      const references = [
        ...source.matchAll(/(?:src|href)=["']~\/(?<asset>[^"'?]+)(?:\?[^"']*)?["']/gi),
        ...source.matchAll(/Url\.Content\(["']~\/(?<asset>[^"']+)["']\)/g)
      ];
      for (const match of references) {
        const asset = match.groups.asset.replaceAll('\\', '/');
        if (asset.includes('@')) continue;
        if (!firstPartyAssets.has(asset)) {
          missing.push(`${normalizedProjectPath(viewFile)} -> ${asset}`);
        }
      }
    }
    expect(missing, missing.join('\n')).toEqual([]);
  });

  it('todo ViewData["PageCSS"] resolve para css/pages com case exato', () => {
    const missing = [];
    for (const viewFile of viewFiles) {
      const source = withoutRazorComments(readFileSync(viewFile, 'utf8'));
      for (const match of source.matchAll(/ViewData\["PageCSS"\]\s*=\s*"(?<name>[^"]+)"/g)) {
        const asset = `css/pages/${match.groups.name}.css`;
        if (!firstPartyAssets.has(asset)) {
          missing.push(`${normalizedProjectPath(viewFile)} -> ${asset}`);
        }
      }
    }
    expect(missing, missing.join('\n')).toEqual([]);
  });
});

describe('contratos de seletores das Views', () => {
  const optionalLegacyIds = new Map([
    ['Views/Comunidade/ComunidadesDetalhes.cshtml', new Set(['confirmationText', 'confirmationError', 'deleteForm'])],
    ['Views/Diariocampo/create.cshtml', new Set(['count-equipe', 'empty-equipe'])],
    ['Views/Diariocampo/edit.cshtml', new Set(['count-equipe', 'empty-equipe'])]
  ]);

  it('IDs literais usados por scripts inline existem na mesma View ou estão documentados como legados', () => {
    const missing = [];
    for (const viewFile of viewFiles) {
      const viewPath = normalizedProjectPath(viewFile);
      const source = withoutRazorComments(readFileSync(viewFile, 'utf8'));
      const ids = renderedIds(source);
      const scripts = [...source.matchAll(/<script(?:\s[^>]*)?>(?<code>[\s\S]*?)<\/script>/gi)].map(x => x.groups.code);
      const allowed = optionalLegacyIds.get(viewPath) ?? new Set();
      for (const script of scripts) {
        for (const match of script.matchAll(/getElementById\(\s*["'](?<id>[^"']+)["']\s*\)/g)) {
          if (!ids.has(match.groups.id) && !allowed.has(match.groups.id)) {
            missing.push(`${viewPath} -> #${match.groups.id}`);
          }
        }
      }
    }
    expect(missing, missing.join('\n')).toEqual([]);
  });

  it('não há IDs literais duplicados dentro de uma View', () => {
    const duplicates = [];
    for (const viewFile of viewFiles) {
      const source = withoutRazorComments(readFileSync(viewFile, 'utf8'));
      const ids = [...source.matchAll(/\bid\s*=\s*["'](?<id>[^"'@<>]+)["']/gi)].map(x => x.groups.id);
      const counts = ids.reduce((map, id) => map.set(id, (map.get(id) ?? 0) + 1), new Map());
      for (const [id, count] of counts) {
        if (count > 1) duplicates.push(`${normalizedProjectPath(viewFile)} -> #${id} (${count}x)`);
      }
    }
    expect(duplicates, duplicates.join('\n')).toEqual([]);
  });

  it('todo label com for literal aponta para um campo da mesma View', () => {
    const orphanLabels = [];
    for (const viewFile of viewFiles) {
      const source = withoutRazorComments(readFileSync(viewFile, 'utf8'));
      const ids = renderedIds(source);
      for (const match of source.matchAll(/<label\b[^>]*\bfor\s*=\s*["'](?<id>[^"'@<>]+)["'][^>]*>/gi)) {
        if (!ids.has(match.groups.id)) orphanLabels.push(`${normalizedProjectPath(viewFile)} -> for="${match.groups.id}"`);
      }
    }
    expect(orphanLabels, orphanLabels.join('\n')).toEqual([]);
  });
});

describe('segurança estrutural dos formulários', () => {
  it('todo formulário POST usa FormTagHelper ou token antifalsificação explícito', () => {
    const unprotected = [];
    for (const viewFile of viewFiles) {
      const source = withoutRazorComments(readFileSync(viewFile, 'utf8'));
      for (const match of source.matchAll(/<form\b(?<attrs>[^>]*)>(?<body>[\s\S]*?)<\/form>/gi)) {
        if (!/\bmethod\s*=\s*["']post["']/i.test(match.groups.attrs)) continue;
        const usesFormTagHelper = /\basp-(?:action|controller|route|page)\b/i.test(match.groups.attrs);
        const hasExplicitToken = /@Html\.AntiForgeryToken\s*\(/.test(match.groups.body)
          || /name\s*=\s*["']__RequestVerificationToken["']/i.test(match.groups.body);
        if (!usesFormTagHelper && !hasExplicitToken) unprotected.push(normalizedProjectPath(viewFile));
      }
    }
    expect(unprotected, unprotected.join('\n')).toEqual([]);
  });
});

describe('contratos dos scripts de página', () => {
  const contracts = [
    {
      view: 'Views/Account/index.cshtml',
      expectations: ['id="showPassword"', 'name="Password"', 'class="login-button"']
    },
    {
      view: 'Views/Account/Forgot.cshtml',
      expectations: ['id="emailInput"', 'id="recoveryBtn"', 'id="successMessage"', 'class="recovery-form"']
    },
    {
      view: 'Views/FichaPrimeiroContato/Create.cshtml',
      expectations: ['class="step-content', 'class="step', 'id="btn-prev"', 'id="btn-next"', 'id="btn-save"']
    },
    {
      view: 'Views/FichaPrimeiroContato/Edit.cshtml',
      expectations: ['class="step-content', 'class="step', 'id="btn-prev"', 'id="btn-next"', 'id="edit-save-btn"', 'id="deleteConfirmationModal"']
    },
    {
      view: 'Views/Diariocampo/create.cshtml',
      expectations: ['id="descricaoInput"', 'id="mention-list"', 'id="modalAcao"', 'id="mapa-diario"']
    },
    {
      view: 'Views/Diariocampo/edit.cshtml',
      expectations: ['id="descricaoInput"', 'id="mention-list"', 'id="modalAcao"', 'id="mapa-diario"']
    }
  ];

  it.each(contracts)('$view preserva os seletores exigidos por seus scripts', ({ view, expectations }) => {
    const source = readProjectFile(view);
    for (const selectorFragment of expectations) expect(source).toContain(selectorFragment);
  });
});
describe('avaliação pessoal — domínio e bindings', () => {
  const metrics = ['CCrimes', 'Substancias', 'Moradia', 'Prevencao', 'AssBasica', 'Educacao', 'Saude', 'Ocupacao', 'Lazer'];
  const assessmentViews = ['Views/PersonalAssessment/Create.cshtml', 'Views/PersonalAssessment/Edit.cshtml'];

  it.each(assessmentViews)('%s renderiza exatamente as nove métricas reais, uma vez cada', view => {
    const source = readProjectFile(view);
    const renderedMetrics = [...source.matchAll(/<input\b[^>]*\bclass=["'][^"']*metric-slider[^"']*["'][^>]*\basp-for=["'](?<metric>[^"']+)["'][^>]*>/gi)]
      .map(match => match.groups.metric);
    expect(renderedMetrics).toHaveLength(9);
    expect(new Set(renderedMetrics).size).toBe(9);
    expect([...renderedMetrics].sort()).toEqual([...metrics].sort());
    for (const metric of metrics) expect(source).toContain(`for="${metric}"`);
    expect(source).not.toMatch(/SeguridadeSocial|RedePrimaria/);
    expect(source).toMatch(/for="CCrimes">Cometimento de crimes/);
  });

  it('Edit vincula o ator real e volta para a listagem do mesmo ator', () => {
    const source = readProjectFile('Views/PersonalAssessment/Edit.cshtml');
    expect(source).toContain('<select asp-for="FK_id_Atores"');
    expect(source).not.toContain('<select asp-for="FkIdUsuario"');
    expect(source).toContain('asp-route-atorId="@Model.FK_id_Atores"');
    expect(source).not.toContain('asp-route-atorId="@Model.IdAvaliacao"');
  });
});

describe('segurança dos templates DOM do diário', () => {
  const diaryFiles = [
    'wwwroot/js/diariocampo.js',
    'wwwroot/js/diariocampo_padrao.js',
    'Views/Diariocampo/create.cshtml',
    'Views/Diariocampo/edit.cshtml'
  ];

  it.each(diaryFiles)('%s não interpola variáveis dentro de templates atribuídos a innerHTML', file => {
    const source = readProjectFile(file);
    const templates = [...source.matchAll(/\.innerHTML\s*=\s*`(?<template>[\s\S]*?)`\s*;/g)];
    for (const template of templates) {
      expect(template.groups.template, `${file} contém interpolação em innerHTML`).not.toContain('${');
    }
    expect(source).not.toMatch(/\bli\.innerHTML\s*=/);
  });

  it('serializa atores com o encoder seguro antes de inserir JSON no script', () => {
    for (const file of ['Views/Diariocampo/create.cshtml', 'Views/Diariocampo/edit.cshtml']) {
      expect(readProjectFile(file)).not.toContain('UnsafeRelaxedJsonEscaping');
    }
  });

  it('o script compartilhado preserva o contrato completo e uma chave reutilizada de TempAcoes', () => {
    const source = readProjectFile('wwwroot/js/diariocampo.js');
    for (const field of ['Nome', 'Provedor', 'Tipo', 'Quantidade', 'FkIdAtor', 'FkIdEixo']) {
      expect(source).toContain(`].${field}`);
    }
    expect(source).toContain("appendHiddenInput('TempAcoes.Index', timestamp)");
  });

  it.each(['Views/Diariocampo/create.cshtml', 'Views/Diariocampo/edit.cshtml'])('%s delega eventos ao script compartilhado sem redeclarar handlers', file => {
    const source = readProjectFile(file);
    expect(source).toContain('<script src="~/js/diariocampo.js"></script>');
    expect(source).toContain('window.atoresDisponiveis');
    expect(source).not.toMatch(/function\s+(?:abrirModalAcao|fecharModal|salvarAcaoNoGrid|removerItemGrid)\s*\(/);
  });
});
describe('atores vinculados — contexto e affordances', () => {
  const source = readProjectFile('Views/Comunidade/AtoresVinculados.cshtml');

  it('preserva a comunidade no link de criação e usa os seletores renderizados', () => {
    expect(source).toContain('asp-route-id="@ViewBag.ComunidadeId"');
    expect(source).not.toContain('asp-route-id="@ViewData["id"]"');
    expect(source).toContain('document.querySelectorAll(".form-item")');
    expect(source).toContain('card.querySelector(".item-name")');
    expect(source).not.toContain('.ator-card');
    expect(source).not.toContain('.ator-name');
  });
});
describe('estrutura HTML e affordances', () => {
  it('Views que usam Layout não declaram um segundo documento HTML', () => {
    const invalid = [];
    for (const viewFile of viewFiles) {
      const viewPath = normalizedProjectPath(viewFile);
      if (viewPath.startsWith('Views/Shared/')) continue;
      const source = withoutRazorComments(readFileSync(viewFile, 'utf8'));
      if (!/\bLayout\s*=\s*"_[^"]+"/.test(source)) continue;
      if (/<!DOCTYPE\s+html|<html\b|<head\b|<body\b/i.test(source)) invalid.push(viewPath);
    }
    expect(invalid, invalid.join('\n')).toEqual([]);
  });

  it('botões conhecidos sem implementação permanecem desabilitados e explicam a limitação', () => {
    const invalid = [];
    for (const viewFile of viewFiles) {
      const source = withoutRazorComments(readFileSync(viewFile, 'utf8'));
      for (const match of source.matchAll(/<button\b(?<attrs>[^>]*)>(?<body>[\s\S]*?)<\/button>/gi)) {
        if (!/\btype\s*=\s*["']button["']/i.test(match.groups.attrs)) continue;
        const signature = `${match.groups.attrs} ${match.groups.body}`;
        const knownPlaceholder = /\b(?:btn-export|atores-export-button|action-card)\b/.test(signature)
          || (/\bbtn-eye\b/.test(signature) && /Adicionar Novo|Importar/.test(signature))
          || /Aplicar\s+Filtros/.test(signature)
          || (/class=["'][^"']*btn-delete/.test(signature) && !/\bid\s*=/.test(match.groups.attrs));
        if (!knownPlaceholder) continue;
        const disabled = /\bdisabled\b/i.test(match.groups.attrs) && /aria-disabled=["']true["']/i.test(match.groups.attrs);
        const explained = /\btitle=["'][^"']+["']/i.test(match.groups.attrs);
        if (!disabled || !explained) invalid.push(`${normalizedProjectPath(viewFile)} -> ${signature.trim().slice(0, 100)}`);
      }
    }
    expect(invalid, invalid.join('\n')).toEqual([]);
  });

  it('nenhum template innerHTML interpola variáveis', () => {
    const files = [
      ...viewFiles,
      ...filesUnder(join(projectRoot, 'wwwroot', 'js'), '.js')
    ];
    const unsafe = [];
    for (const file of files) {
      const source = readFileSync(file, 'utf8');
      for (const match of source.matchAll(/\.innerHTML\s*=\s*`(?<template>[\s\S]*?)`\s*;/g)) {
        if (match.groups.template.includes('${')) unsafe.push(normalizedProjectPath(file));
      }
    }
    expect(unsafe, unsafe.join('\n')).toEqual([]);
  });
});
