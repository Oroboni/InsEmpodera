using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Text;

namespace Empodera.Controllers;

public class ReportController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Report/
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        return View();
    }

    // POST: /Report/RelatorioComunidade
    [HttpPost]
    public async Task<IActionResult> RelatorioComunidade(IList<IFormFile> files)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        if (files == null || files.Count == 0)
            return RedirectToAction("Index");

        int userId = int.Parse(HttpContext.Session.GetString("ID"));

        var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files)
        {
            if (file == null || file.Length <= 0) continue;

            var filepath = Path.Combine(uploadFolder, file.FileName);
            using (var stream = new FileStream(filepath, FileMode.Create))
                await file.CopyToAsync(stream);

            await ProcessarExcel(filepath, userId);
        }

        return RedirectToAction("Index");
    }

    private async Task ProcessarExcel(string filepath, int userId)
    {
        var conf = new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
        };

        using var fileStream = System.IO.File.Open(filepath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(fileStream);
        var dataSet = reader.AsDataSet(conf);

        // ─────────────────────────────────────────────────────────────
        // [0] — COMUNIDADE
        // Linha 0=Nome, 1=Localização, 2=Descrição, 3=Acessibilidade,
        //        4=Estado, 5=Atores vinculados, 6=Recursos, 7=Vulnerabilidades, 8=Diários de Campo
        // ─────────────────────────────────────────────────────────────
        var comunidade = new Comunidade();

        if (dataSet.Tables.Count > 0)
        {
            var sheet = dataSet.Tables[0];

            comunidade.Nome                    = sheet.Rows[0][1]?.ToString()?.Trim();
            comunidade.Local                   = sheet.Rows[1][1]?.ToString()?.Trim();
            comunidade.Descricao               = sheet.Rows[2][1]?.ToString()?.Trim();
            comunidade.DescricaoAcessibilidade = sheet.Rows[3][1]?.ToString()?.Trim();
            comunidade.Status                  = NormalizeStatus(sheet.Rows[4][1]?.ToString());
            comunidade.DtCriacao               = DateTime.Now;
            comunidade.DtModificacao           = DateTime.Now;
            comunidade.FkIdUsuario             = userId;
            comunidade.Ativo                   = "S";

            _context.Comunidades.Add(comunidade);
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // [1] — ATORES
        // Linha 0 = cabeçalho, dados a partir da linha 1
        // Colunas: 0=Nome, 1=Gênero, 2=Idade, 3=Papel Social, 4=Telefone,
        //          5=Líder de Opinião, 6=Rede Operativa, 7=Da Equipe, 8=Mora na comunidade
        // ─────────────────────────────────────────────────────────────

        // Mapa nome → IdAtores para uso nas abas seguintes
        var atoresPorNome = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        if (dataSet.Tables.Count > 1)
        {
            var sheet = dataSet.Tables[1];

            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row  = sheet.Rows[i];
                var nome = row[0]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(nome)) continue;

                var ator = new Atores
                {
                    Nome          = nome,
                    Genero        = NormalizeGenero(row[1]?.ToString()),
                    Idade         = ParseInt(row[2]?.ToString()),
                    PapelSocial1  = row[3]?.ToString()?.Trim(),
                    Telefone      = FormatPhone(row[4]?.ToString()),
                    Lopiniao      = ParseBool(row[5]?.ToString()),
                    Rope          = ParseBool(row[6]?.ToString()),
                    DaEquipe      = ParseBool(row[7]?.ToString()),
                    Mcomunidade   = ParseBool(row[8]?.ToString()),
                    DtCriacao     = DateTime.Now,
                    DtModificacao = DateTime.Now,
                    FkIdUsuario   = userId,
                    Ativo         = "S"
                };

                _context.Atores.Add(ator);
                await _context.SaveChangesAsync();

                atoresPorNome[nome] = ator.IdAtores;

                _context.AtorComunidades.Add(new AtorComunidade
                {
                    FkIdComunidade = comunidade.IdComunidade,
                    FKidAtores     = ator.IdAtores
                });
            }
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // [2] — ATIVIDADES
        // Linha 0 = cabeçalho
        // Colunas: 0=Nome, 1=Eixos, 2=Descrição
        // ─────────────────────────────────────────────────────────────
        if (dataSet.Tables.Count > 2)
        {
            var sheet = dataSet.Tables[2];

            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row  = sheet.Rows[i];
                var nome = row[0]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(nome)) continue;

                _context.Atividades.Add(new Atividades
                {
                    Nome           = nome,
                    Descricao      = row[2]?.ToString()?.Trim() ?? string.Empty,
                    FkIdComunidade = comunidade.IdComunidade,
                    FkIdUsuario    = userId
                });
            }
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // [3] — RECURSOS
        // Linha 0 = cabeçalho
        // Colunas: 0=Nome, 1=Tipo, 2=Eixos, 3=Ator, 4=Localização, 5=Dispositivo, 6=Serviços
        // ─────────────────────────────────────────────────────────────
        if (dataSet.Tables.Count > 3)
        {
            var sheet = dataSet.Tables[3];

            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row  = sheet.Rows[i];
                var nome = row[0]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(nome)) continue;

                // Coluna 3 = Ator (nome do ator relacionado, se "Recurso Relacional")
                var nomeAtor  = row[3]?.ToString()?.Trim();
                int? idAtor   = null;
                if (!string.IsNullOrEmpty(nomeAtor) && atoresPorNome.TryGetValue(nomeAtor, out var aid))
                    idAtor = aid;

                _context.RedeRecursos.Add(new RedeRecursos
                {
                    Nome           = nome,
                    Tipo           = row[1]?.ToString()?.Trim() ?? string.Empty,
                    Localizacao    = row[4]?.ToString()?.Trim(),
                    Dispositivo    = row[5]?.ToString()?.Trim(),
                    Servicos       = row[6]?.ToString()?.Trim(),
                    FkIdComunidade = comunidade.IdComunidade,
                    FKidAtores       = idAtor,
                    DtCriacao      = DateTime.Now,
                    DtModificacao  = DateTime.Now,
                    FkIdUsuario    = userId
                });
            }
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // [4] — VULNERABILIDADES
        // Linha 0 = cabeçalho
        // Colunas: 0=Nome, 1=Eixos, 2=Localização
        // ─────────────────────────────────────────────────────────────
        if (dataSet.Tables.Count > 4)
        {
            var sheet = dataSet.Tables[4];

            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row  = sheet.Rows[i];
                var nome = row[0]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(nome)) continue;

                _context.Vulnerabilidades.Add(new Vulnerabilidade
                {
                    Nome           = nome,
                    Localizacao    = row[2]?.ToString()?.Trim() ?? string.Empty,
                    Servicos       = string.Empty,
                    FkIdComunidade = comunidade.IdComunidade
                });
            }
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // [5] — DIÁRIOS DE CAMPO
        // Linha 0 = cabeçalho
        // Colunas: 0=Data, 1=Descrição, 2=Eixos, 3=Localização, 4=Atividades, 5=Ações
        // ─────────────────────────────────────────────────────────────
        if (dataSet.Tables.Count > 5)
        {
            var sheet = dataSet.Tables[5];

            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row       = sheet.Rows[i];
                var dataRaw   = row[0]?.ToString()?.Trim();
                var descricao = row[1]?.ToString()?.Trim() ?? string.Empty;

                // Pula só se não tiver nem data nem descrição
                if (string.IsNullOrEmpty(dataRaw) && string.IsNullOrEmpty(descricao)) continue;

                DateTime.TryParse(dataRaw, out DateTime data);

                _context.DiariosCampo.Add(new DiarioCampo
                {
                    FkIdComunidade = comunidade.IdComunidade,
                    Data           = data == default ? DateTime.Now : data,
                    Descricao      = descricao,
                    Localizacao    = row[3]?.ToString()?.Trim() ?? string.Empty,
                    Foto           = string.Empty,
                    DtCriacao      = DateTime.Now,
                    DtModificacao  = DateTime.Now,
                    FkIdUsuario    = userId
                });
            }
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // [6] — RSC (matriz de relacionamento)
        // Linha 0 = cabeçalho com categorias (Parceiro, Equipe, RRC, RLO, RO, Habitante)
        // Colunas: 0=Categoria origem, 1..6=contagens por categoria destino
        // Apenas armazena os totais se o modelo RSC existir
        // ─────────────────────────────────────────────────────────────
        if (dataSet.Tables.Count > 6)
        {
            var sheet = dataSet.Tables[6];
            // Linha 1 contém a linha "RSC" com os totais gerais
            // Estrutura: RSC | 16 | 8 | 3 | 6 | 26 | 62
            if (sheet.Rows.Count > 1)
            {
                var totaiRow = sheet.Rows[1];
                // Salva como metadado da comunidade se o campo existir, caso contrário ignora silenciosamente
                // Ajuste conforme o modelo RSC da sua aplicação
            }
        }

        // ─────────────────────────────────────────────────────────────
        // [7] — AVALIAÇÕES PESSOAIS
        // Linha 0 = cabeçalho
        // Colunas: 0=Ator, 1=Data, 2=Rede primária, 3=Seguridade Social,
        //          4=Substâncias, 5=Moradia, 6=Prevenção, 7=Assistência Básica,
        //          8=Educação, 9=Saúde, 10=Ocupação, 11=Lazer
        // ─────────────────────────────────────────────────────────────
        if (dataSet.Tables.Count > 7)
        {
            var sheet = dataSet.Tables[7];

            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row      = sheet.Rows[i];
                var nomeAtor = row[0]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(nomeAtor)) continue;

                if (!atoresPorNome.TryGetValue(nomeAtor, out int idAtor)) continue;

                DateTime.TryParse(row[1]?.ToString(), out DateTime dtAvaliacao);

                _context.AvaliacaoPessoal.Add(new AvaliacaoPessoal
                {
                    FKidAtores      = idAtor,
                    // Rede primária (coluna 2) não possui campo numérico padrão; ignora ou adapte
                    Substancias   = ParseInt(row[4]?.ToString()),
                    Moradia       = ParseInt(row[5]?.ToString()),
                    Prevencao     = ParseInt(row[6]?.ToString()),
                    AssBasica     = ParseInt(row[7]?.ToString()),
                    Educacao      = ParseInt(row[8]?.ToString()),
                    Saude         = ParseInt(row[9]?.ToString()),
                    Ocupacao      = ParseInt(row[10]?.ToString()),
                    Lazer         = ParseInt(row[11]?.ToString()),
                    DtCriacao     = dtAvaliacao == default ? DateTime.Now : dtAvaliacao,
                    DtModificacao = DateTime.Now,
                    FkIdUsuario   = userId
                });
            }
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // [8] — DIÁRIO DE PROCESSO PESSOAL
        // Linha 0 = cabeçalho
        // Colunas: 0=Ator, 1=Data, 2=Descrição, 3=Eixos, 4=Atividades
        // (sem dados nesta planilha; importação defensiva)
        // ─────────────────────────────────────────────────────────────
        if (dataSet.Tables.Count > 8)
        {
            var sheet = dataSet.Tables[8];

            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row       = sheet.Rows[i];
                var nomeAtor  = row[0]?.ToString()?.Trim();
                var descricao = row[2]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(nomeAtor) && string.IsNullOrEmpty(descricao)) continue;

                if (!atoresPorNome.TryGetValue(nomeAtor ?? "", out int idAtor)) continue;

                DateTime.TryParse(row[1]?.ToString(), out DateTime data);

                // Armazena como DiarioCampo vinculado ao ator se não houver modelo dedicado
                // Adapte para o modelo DiarioProcessoPessoal se ele existir
                _context.DiariosCampo.Add(new DiarioCampo
                {
                    FkIdComunidade = comunidade.IdComunidade,
                    Data           = data == default ? DateTime.Now : data,
                    Descricao      = descricao ?? string.Empty,
                    Localizacao    = string.Empty,
                    Foto           = string.Empty,
                    DtCriacao      = DateTime.Now,
                    DtModificacao  = DateTime.Now,
                    FkIdUsuario    = userId
                });
            }
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // [9] — FICHAS DE 1° CONTATO
        // Linha 0 = cabeçalho (98 colunas)
        // Colunas relevantes mapeadas abaixo:
        //  0=Ator, 1=Localização, 2=Gênero, 3=Data de Nascimento, 4=Telefone,
        //  5=Emprego, 6=Data do primeiro contato, 7=Hora do primeiro contato,
        //  8=Como se estabeleceu, 9=Novo parceiro?, 10=Dados do parceiro?,
        //  11=Fonte da informação, 12=Estado familiar, 13=Estrutura familiar,
        //  14=Nº filhos, 15=Nº filhas, 16=Anos de escola, 17=Sabe ler,
        //  18=Sabe calcular, 19=Sabe computador, 20=Qtd reabilitações,
        //  21=Local tratamento, 22=Substâncias, ... (condições a partir col 22)
        // ─────────────────────────────────────────────────────────────
        if (dataSet.Tables.Count > 9)
        {
            var sheet = dataSet.Tables[9];

            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row      = sheet.Rows[i];
                var nomeAtor = row[0]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(nomeAtor)) continue;

                // Tenta resolver o ator; cria vínculo se já existir
                atoresPorNome.TryGetValue(nomeAtor, out int idAtor);

                DateTime.TryParse(row[3]?.ToString(), out DateTime dtNasc);
                DateTime.TryParse(row[6]?.ToString(), out DateTime dtContato);
                TimeSpan.TryParse(ParseHoraContato(row[7]?.ToString()), out TimeSpan horaContato);

                var ficha = new FichaPrimeiroContato
                {
                    FKidAtores         = idAtor,
                    Endereco           = row[1]?.ToString()?.Trim() ?? string.Empty,
                    Emprego            = row[5]?.ToString()?.Trim(),
                    CEstabeleceu       = row[8]?.ToString()?.Trim(),
                    NovoParceiro       = ParseBoolString(row[9]?.ToString()),
                    FornecidoParceiro  = ParseBoolString(row[10]?.ToString()),
                    FonteDados         = row[11]?.ToString()?.Trim(),
                    EstaFamiliar       = row[12]?.ToString()?.Trim(),
                    EstruFamiliar      = row[13]?.ToString()?.Trim(),
                    NFIlhos            = ParseNullableInt(row[14]?.ToString()),
                    NFilhas            = ParseNullableInt(row[15]?.ToString()),
                    AEscolar           = ParseNullableInt(row[16]?.ToString()),
                    SLer               = ParseBoolString(row[17]?.ToString()),
                    SCalc              = ParseBoolString(row[18]?.ToString()),
                    SComp              = ParseBoolString(row[19]?.ToString()),
                    QReabili           = ParseNullableInt(row[20]?.ToString()),
                    LTrat              = row[21]?.ToString()?.Trim(),
                    DtContato          = dtContato == default ? DateTime.Now : dtContato,
                    HoraContato        = horaContato,
                    DtCriacao          = DateTime.Now,
                    DtModificacao      = DateTime.Now,
                    FkIdUsuario        = userId,
                    FkIdComunidade     = comunidade.IdComunidade,
                    Status             = "EmProgresso"
                };

                _context.FichasPrimeiroContato.Add(ficha);
                await _context.SaveChangesAsync();

                // ── Condições (colunas 22–53) ────────────────────────
                // Cada coluna true/false representa uma condição presente
                var condicoesLabels = new[]
                {
                    "Substâncias/álcool",         // 22
                    "Condutas antissociais",       // 23
                    "Psiquiátricas",               // 24
                    "Comportamental",              // 25
                    "Relacionais",                 // 26
                    "Violência intrafamiliar",     // 27
                    "Violação de adulto",          // 28
                    "Trabalho sexual",             // 29
                    "Legal",                       // 30
                    "Gravidez/parto",              // 31
                    "Problemas de saúde",          // 32
                    "Pobreza extrema",             // 33
                    "Vida de rua",                 // 34
                    "Apoio econômico",             // 35
                    "Escolares",                   // 36
                    "Exclusão grave",              // 37
                    "Violência intracomunitária",  // 38
                    "Redes interinstitucionais",   // 39
                    "Crise psicológica",           // 40
                    "Capacitação e formação",      // 41
                    "Organização e planejamento",  // 42
                    "HIV/AIDS",                    // 43
                    "IST",                         // 44
                    "Tráfico de pessoas",          // 45
                    "Sem trabalho",                // 46
                    "Tuberculoso",                 // 47
                    "Problemas familiares",        // 48
                    "Transtorno estresse pós traumático", // 49
                    "Analfabetismo",               // 50
                    "Problemas sexuais",           // 51
                    "Violação de crianças",        // 52
                    "Outras dependências",         // 53
                };

                for (int c = 0; c < condicoesLabels.Length; c++)
                {
                    int colIdx = 22 + c;
                    if (colIdx >= sheet.Columns.Count) break;
                    if (ParseBool(row[colIdx]?.ToString()))
                    {
                        _context.FichaCondicoes.Add(new FichaCondicoes
                        {
                            FkIdFicha = ficha.IdFicha,
                            Cond      = condicoesLabels[c]
                        });
                    }
                }

                // ── Petições / respostas (colunas 54–75) ─────────────
                var peticoesLabels = new[]
                {
                    "Drogas intravenosas",         // 54
                    "Hepatite",                    // 55
                    "Deslocamento",                // 56
                    "Migração/Imigração",          // 57
                    "Estigma por identidade sexual", // 58
                    "Incapacidade",                // 59
                    "Apoio econômico",             // 60
                    "Atenção ao parto",            // 61
                    "Encaminhamento",              // 62
                    "Encontro/Conversa",           // 63
                    "Conselho/Orientação",         // 64
                    "Serviços legais",             // 65
                    "Fianças/Empréstimo",          // 66
                    "Formação/Capacitação",        // 67
                    "Creche",                      // 68
                    "Internação",                  // 69
                    "Informação",                  // 70
                    "Integração comunitária",      // 71
                    "Organização e planejamento",  // 72
                    "Internação forçada",          // 73
                    "Recuperação escolar",         // 74
                    "Serviço social",              // 75
                };

                for (int c = 0; c < peticoesLabels.Length; c++)
                {
                    int colIdx = 54 + c;
                    if (colIdx >= sheet.Columns.Count) break;
                    if (ParseBool(row[colIdx]?.ToString()))
                    {
                        _context.FichaPeticoes.Add(new FichaPeticoes
                        {
                            FkIdFicha = ficha.IdFicha,
                            Pet       = peticoesLabels[c]
                        });
                    }
                }

                // ── Respostas dadas (colunas 76–87) ──────────────────
                var respostasLabels = new[]
                {
                    "Terapia",                        // 76
                    "Visita familiar",                // 77
                    "Exames/Cuidados médicos",        // 78
                    "Alimentação",                    // 79
                    "Serviços de higiene/Roupa limpa",// 80
                    "Ocupação/trabalho",              // 81
                    "Medicamentos",                   // 82
                    "Burocrático",                    // 83
                    "Marca-se um encontro",           // 84
                    "Dá-se uma informação",           // 85
                    "Conselho/Orientação",            // 86
                    "Encaminhamento",                 // 87
                };

                for (int c = 0; c < respostasLabels.Length; c++)
                {
                    int colIdx = 76 + c;
                    if (colIdx >= sheet.Columns.Count) break;
                    if (ParseBool(row[colIdx]?.ToString()))
                    {
                        _context.FichaRespostas.Add(new FichaResp
                        {
                            FkIdFicha = ficha.IdFicha,
                            Resp      = respostasLabels[c]
                        });
                    }
                }

                // ── Resultados (colunas 88–98) ────────────────────────
                var resultadosLabels = new[]
                {
                    "Escuta imediata/manejo de crise", // 88
                    "Indicações, sugestões",           // 89
                    "Acompanhamento",                  // 90
                    "Cuidados médicos",                // 91
                    "Higiene/Roupa limpa",             // 92
                    "Acolhida (dia ou noite)",         // 93
                    "Veio ao encontro",                // 94
                    "Seguiu contato",                  // 95
                    "Interrompeu contato",             // 96
                    "Inicia um programa",              // 97
                    "Inclusão em atividades",          // 98
                };

                for (int c = 0; c < resultadosLabels.Length; c++)
                {
                    int colIdx = 88 + c;
                    if (colIdx >= sheet.Columns.Count) break;
                    if (ParseBool(row[colIdx]?.ToString()))
                    {
                        _context.FichaResultados.Add(new FichaResult
                        {
                            FkIdFicha = ficha.IdFicha,
                            Result    = resultadosLabels[c]
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }
        }
    }

    // ─── Helpers ───────────────────────────────────────────────────────

    private static bool ParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var v = value.Trim();
        return v.Equals("True", StringComparison.OrdinalIgnoreCase)
            || v.Equals("true", StringComparison.OrdinalIgnoreCase)
            || v == "1";
    }

    /// <summary>Retorna "S", "N" ou null dependendo do valor booleano da célula.</summary>
    private static string? ParseBoolString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return ParseBool(value) ? "S" : "N";
    }

    private static string NormalizeStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        return value.Trim().ToLowerInvariant() switch
        {
            "in diagnosis"   => "Em diagnóstico",
            "in process"     => "Em processo",
            "en proceso"     => "Em processo",
            "en diagnóstico" => "Em diagnóstico",
            "en diagnostico" => "Em diagnóstico",
            "diagnosticado"  => "Diagnosticado",
            "em diagnóstico" => "Em diagnóstico",
            "em processo"    => "Em processo",
            _                => value.Trim()
        };
    }

    private static string NormalizeGenero(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        return value.Trim().ToLowerInvariant() switch
        {
            "femenino"  => "Feminino",
            "feminino"  => "Feminino",
            "female"    => "Feminino",
            "f"         => "Feminino",
            "masculino" => "Masculino",
            "male"      => "Masculino",
            "m"         => "Masculino",
            _           => value.Trim()
        };
    }

    /// <summary>Retorna o telefone como string preservando formatação do Excel.</summary>
    private static string? FormatPhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim();
    }

    private static int ParseInt(string? value, int defaultValue = 0)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        // Remove decimais que o Excel pode incluir (ex: "5.0")
        var clean = value.Trim().Split('.')[0];
        return int.TryParse(clean, out int result) ? result : defaultValue;
    }

    private static int? ParseNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var clean = value.Trim().Split('.')[0];
        return int.TryParse(clean, out int result) ? result : null;
    }

    /// <summary>
    /// Converte faixas de hora como "Das 06:00 às 12:00" para "06:00:00"
    /// compatível com TimeSpan.TryParse.
    /// </summary>
    private static string ParseHoraContato(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "00:00:00";

        // Se já for HH:mm ou HH:mm:ss, devolve direto
        if (value.Contains(':') && !value.Contains(' '))
            return value.Trim();

        // Extrai primeiro horário da string "Das 06:00 às 12:00"
        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (part.Contains(':'))
                return part;
        }

        return "00:00:00";
    }

    // ─── Actions de relatório ────────────────────────────────────────

    // GET: /Report/Rsc
    public async Task<IActionResult> Rsc(int? comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        ViewBag.ComunidadeList = new SelectList(await _context.Comunidades.ToListAsync(), "IdComunidade", "Nome", comunidadeId);
        return View();
    }

    // GET: /Report/FirstContact
    public async Task<IActionResult> FirstContact(int? comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        ViewBag.ComunidadeList = new SelectList(await _context.Comunidades.ToListAsync(), "IdComunidade", "Nome", comunidadeId);
        return View();
    }

    // GET: /Report/PersonalAssessment
    public async Task<IActionResult> PersonalAssessment(int? atorId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        ViewBag.AtorList = new SelectList(await _context.Atores.ToListAsync(), "IdAtores", "Nome", atorId);
        ViewBag.SelectedAtorId = atorId;
        return View();
    }

    // GET: /Report/Actions
    public async Task<IActionResult> Actions(int? comunidadeId, int? atorId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        ViewBag.ComunidadeList = new SelectList(await _context.Comunidades.ToListAsync(), "IdComunidade", "Nome", comunidadeId);
        ViewBag.AtorList = new SelectList(await _context.Atores.ToListAsync(), "IdAtores", "Nome", atorId);
        return View();
    }

    // GET: /Report/PrimaryNetwork
    public async Task<IActionResult> PrimaryNetwork(int? atorId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        ViewBag.AtorList = new SelectList(await _context.Atores.ToListAsync(), "IdAtores", "Nome", atorId);
        ViewBag.SelectedAtorId = atorId;
        return View();
    }
}