using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ExcelDataReader;
using System.Text;

namespace Empodera.Controllers;

public class ReporteController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReporteController(ApplicationDbContext context)
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

        // ─────────────────────────────────────────
        // [0] — COMMUNITY / COMUNIDADE
        // Linha 0=Nome, 1=Local, 2=Descrição, 3=Acessibilidade, 4=Estado
        // ─────────────────────────────────────────
        var comunidade = new Comunidade();

        if (dataSet.Tables.Count > 0)
        {
            var sheet = dataSet.Tables[0];

            comunidade.Nome                    = sheet.Rows[0][1]?.ToString()?.Trim();
            comunidade.Local                   = sheet.Rows[1][1]?.ToString()?.Trim();
            comunidade.Descricao               = sheet.Rows[2][1]?.ToString()?.Trim();
            comunidade.DescricaoAcessibilidade = sheet.Rows[3][1]?.ToString()?.Trim();
            comunidade.Status                  = sheet.Rows[4][1]?.ToString()?.Trim();
            comunidade.DtCriacao               = DateTime.Now;
            comunidade.DtModificacao           = DateTime.Now;
            comunidade.FkIdUsuario             = userId;
            comunidade.Ativo                   = "S";

            _context.Comunidades.Add(comunidade);
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // [1] — ACTORS / ATORES
        // Linha 0 = cabeçalho, dados a partir da linha 1
        // Colunas: 0=Nome, 1=Gênero, 2=Idade, 3=Papel Social, 4=Telefone,
        //          5=Líder de Opinião, 6=Rede Operativa, 7=Da Equipe, 8=Mora na comunidade
        // ─────────────────────────────────────────
        if (dataSet.Tables.Count > 1)
        {
            var sheet = dataSet.Tables[1];

            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row = sheet.Rows[i];
                var nome = row[0]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(nome)) continue;

                var ator = new Atores
                {
                    Nome          = nome,
                    Genero        = row[1]?.ToString()?.Trim(),
                    PapelSocial1  = row[3]?.ToString()?.Trim(),
                    Telefone      = TryParsePhone(row[4]?.ToString())?.ToString(),
                    Lopiniao      = ParseBool(row[5]?.ToString()),
                    Rope          = ParseBool(row[6]?.ToString()),
                    DaEquipe      = ParseBool(row[7]?.ToString()),
                    Mcomunidade   = ParseBool(row[8]?.ToString()),
                    DtCriacao     = DateTime.Now,
                    DtModificacao = DateTime.Now,
                    FkIdUsuario   = userId,
                    Ativo         = "S"
                };

                if (int.TryParse(row[2]?.ToString(), out int idade))
                    ator.DtNascimento = DateTime.Now.AddYears(-idade);

                _context.Atores.Add(ator);
                await _context.SaveChangesAsync();

                _context.AtorComunidades.Add(new AtorComunidade
                {
                    FkIdComunidade = comunidade.IdComunidade,
                    FKidAtores     = ator.IdAtores
                });
            }
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // [2] — ACTIVITIES / ATIVIDADES
        // Linha 0 = cabeçalho
        // Colunas: 0=Nome, 1=Eixos, 2=Descrição
        // ─────────────────────────────────────────
        if (dataSet.Tables.Count > 2)
        {
            var sheet = dataSet.Tables[2];

            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row = sheet.Rows[i];
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

        // ─────────────────────────────────────────
        // [3] — RESOURCES / RECURSOS
        // Linha 0 = cabeçalho
        // Colunas: 0=Nome, 1=Tipo, 2=Eixos, 3=Ator, 4=Localização, 5=Dispositivo, 6=Serviços
        // ─────────────────────────────────────────
        if (dataSet.Tables.Count > 3)
        {
            var sheet = dataSet.Tables[3];

            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row = sheet.Rows[i];
                var nome = row[0]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(nome)) continue;

                _context.RedeRecursos.Add(new RedeRecursos
                {
                    Nome           = nome,
                    Tipo           = row[1]?.ToString()?.Trim() ?? string.Empty,
                    Localizacao    = row[4]?.ToString()?.Trim(),
                    Dispositivo    = row[5]?.ToString()?.Trim(),
                    Servicos       = row[6]?.ToString()?.Trim(),
                    FkIdComunidade = comunidade.IdComunidade,
                    DtCriacao      = DateTime.Now,
                    DtModificacao  = DateTime.Now,
                    FkIdUsuario    = userId
                });
            }
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // [4] — VULNERABILITIES / VULNERABILIDADES
        // Linha 0 = cabeçalho
        // Colunas: 0=Nome, 1=Eixos, 2=Localização
        // ─────────────────────────────────────────
        if (dataSet.Tables.Count > 4)
        {
            var sheet = dataSet.Tables[4];

            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row = sheet.Rows[i];
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

        // ─────────────────────────────────────────
        // [5] — FIELD JOURNALS / DIÁRIOS DE CAMPO
        // Linha 0 = cabeçalho
        // Colunas: 0=Data, 1=Descrição, 2=Eixos, 3=Localização, 4=Atividades, 5=Ações
        // ─────────────────────────────────────────
        if (dataSet.Tables.Count > 5)
        {
            var sheet = dataSet.Tables[5];

            for (int i = 1; i < sheet.Rows.Count; i++)
            {
                var row = sheet.Rows[i];
                var descricao = row[1]?.ToString()?.Trim();
                if (string.IsNullOrEmpty(descricao)) continue;

                DateTime.TryParse(row[0]?.ToString(), out DateTime data);

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
    }

    // ─── Helpers ───────────────────────────────
    private static bool ParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return value.Trim().Equals("True", StringComparison.OrdinalIgnoreCase)
            || value.Trim() == "1";
    }

    private static int? TryParsePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out int result) ? result : null;
    }

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