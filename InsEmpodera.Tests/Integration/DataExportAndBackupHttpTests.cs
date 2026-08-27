using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ClosedXML.Excel;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class DataExportAndBackupHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private const string SpreadsheetMediaType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private readonly EmpoderaWebApplicationFactory _factory;

    public DataExportAndBackupHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Theory(DisplayName = "Exportações protegidas redirecionam visitantes para o login")]
    [InlineData("/Services/ExportComunidades")]
    [InlineData("/Services/ExportComunidadeCompleta?id=1")]
    [InlineData("/Services/ExportAtoresComunidade?id=1")]
    [InlineData("/Services/ExportAtividadesComunidade?id=1")]
    [InlineData("/Services/ExportRecursosComunidade?id=1")]
    [InlineData("/Services/ExportAtores")]
    [InlineData("/Services/ExportDiariosCampo")]
    [InlineData("/Services/ExportFichasPrimeiroContato")]
    [InlineData("/Services/ExportBackupGeral")]
    public async Task ExportEndpoints_AnonymousAccessAlwaysRedirectsToLogin(string path)
    {
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account", response.Headers.Location?.OriginalString);
        Assert.NotEqual(SpreadsheetMediaType, response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task CompleteCommunityExport_ContainsEveryDependencyGroupAndPrimaryKeyHeaders()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var communityId = await SeedCommunityAsync();
        using var client = await AuthenticatedClientAsync(admin);

        using var response = await client.GetAsync($"/Services/ExportComunidadeCompleta?id={communityId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(SpreadsheetMediaType, response.Content.Headers.ContentType?.MediaType);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var workbook = new XLWorkbook(stream);
        var expectedSheets = new[]
        {
            "Comunidades", "Atores", "AtorComunidades", "RecursosAtores", "AvaliacoesPessoais",
            "RedesPrimarias", "Atividades", "AtividadesEixos", "Acoes", "AcoesAtores", "Recursos",
            "RecursosEixos", "Vulnerabilidades", "VulnerabilidadesEixos", "DiariosCampo", "DiariosEixos",
            "AcoesInstitucionais", "DetalhesAcoes", "EixosDasAcoes", "AtoresDasAcoes", "AcoesDaEquipe",
            "AnexosDiario", "FichasPrimeiroContato", "FontesInformacao", "Condicoes", "Peticoes",
            "Respostas", "Resultados", "Eixos"
        };
        Assert.Equal(expectedSheets, workbook.Worksheets.Select(sheet => sheet.Name));
        Assert.Equal(nameof(Comunidade.Id_Comunidade), workbook.Worksheet("Comunidades").Cell(1, 1).GetString());
        Assert.Equal(communityId, workbook.Worksheet("Comunidades").Cell(2, 1).GetValue<int>());
    }

    [Theory]
    [InlineData("/Services/ExportAtores", "Atores", "RedesPrimarias")]
    [InlineData("/Services/ExportDiariosCampo", "DiariosCampo", "AnexosDiario")]
    [InlineData("/Services/ExportFichasPrimeiroContato", "FichasPrimeiroContato", "Resultados")]
    public async Task ModuleExports_DownloadValidSpreadsheets(string path, string firstSheet, string dependencySheet)
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = await AuthenticatedClientAsync(admin);

        using var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(SpreadsheetMediaType, response.Content.Headers.ContentType?.MediaType);
        Assert.EndsWith(".xlsx", response.Content.Headers.ContentDisposition?.FileNameStar, StringComparison.OrdinalIgnoreCase);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var workbook = new XLWorkbook(stream);
        Assert.Contains(workbook.Worksheets, sheet => sheet.Name == firstSheet);
        Assert.Contains(workbook.Worksheets, sheet => sheet.Name == dependencySheet);
        Assert.All(workbook.Worksheets, sheet => Assert.True(sheet.Row(1).CellCount() > 0));
    }

    [Fact]
    public async Task CommunityActorExport_ContainsCommunityActorsAndTheirRelationships()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var communityId = await SeedCommunityAsync();
        using var client = await AuthenticatedClientAsync(admin);

        using var response = await client.GetAsync($"/Services/ExportAtoresComunidade?id={communityId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var workbook = new XLWorkbook(stream);
        Assert.Equal(
            new[] { "Atores", "AtoresRelacionados", "VinculosComunidade", "RecursosAtores", "AvaliacoesPessoais", "RedesPrimarias" },
            workbook.Worksheets.Select(sheet => sheet.Name));
    }

    [Fact]
    public async Task ScreenExports_DownloadTheVisibleModuleWithItsDependencies()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var communityId = await SeedCommunityAsync();
        using var client = await AuthenticatedClientAsync(admin);
        var exports = new[]
        {
            (Path: "/Services/ExportComunidades", Sheets: new[] { "Comunidades" }),
            (Path: $"/Services/ExportAtividadesComunidade?id={communityId}", Sheets: new[] { "Atividades", "AtividadesEixos", "Acoes", "AcoesAtores", "Atores", "Eixos" }),
            (Path: $"/Services/ExportRecursosComunidade?id={communityId}", Sheets: new[] { "Recursos", "RecursosEixos", "Atores", "Eixos" })
        };

        foreach (var export in exports)
        {
            using var response = await client.GetAsync(export.Path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(SpreadsheetMediaType, response.Content.Headers.ContentType?.MediaType);
            Assert.EndsWith(".xlsx", response.Content.Headers.ContentDisposition?.FileNameStar, StringComparison.OrdinalIgnoreCase);
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var workbook = new XLWorkbook(stream);
            Assert.Equal(export.Sheets, workbook.Worksheets.Select(sheet => sheet.Name));
            Assert.All(workbook.Worksheets, sheet => Assert.True(sheet.Row(1).CellCount() > 0));
        }
    }

    [Fact]
    public async Task GeneralBackup_ContainsAllMappedTablesAndCanBeImportedBackWithoutChangingExistingRows()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var actorName = $"Ator João — inclusão 🌎 {Guid.NewGuid():N}";
        var longDescription = string.Concat(Enumerable.Repeat("Informação comunitária com acentos. ", 1_200));
        var restoredActorId = await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var actor = new Atores
            {
                Nome = actorName,
                PapelSocial1 = longDescription,
                PapelSocial2 = null,
                Telefone = string.Empty,
                DaEquipe = true,
                Rope = false,
                Lopiniao = true,
                Mcomunidade = false,
                Ativo = "S",
                FkIdUsuario = admin.IdUsuario,
                DtCriacao = DateTime.UtcNow,
                DtModificacao = DateTime.UtcNow
            };
            db.Atores.Add(actor);
            await db.SaveChangesAsync();
            return actor.IdAtores;
        });
        using var client = await AuthenticatedClientAsync(admin);
        var countsBefore = await GetTableCountsAsync();

        using (var actorExport = await client.GetAsync("/Services/ExportAtores"))
        {
            Assert.Equal(HttpStatusCode.OK, actorExport.StatusCode);
            await using var actorStream = await actorExport.Content.ReadAsStreamAsync();
            using var actorWorkbook = new XLWorkbook(actorStream);
            var longTexts = actorWorkbook.Worksheet("TextosLongos");
            var reconstructed = string.Concat(longTexts.RowsUsed().Skip(1)
                .Where(row => row.Cell(1).GetString() == "Atores" && row.Cell(3).GetString() == nameof(Atores.PapelSocial1))
                .OrderBy(row => row.Cell(4).GetValue<int>())
                .Select(row => row.Cell(5).GetString()));
            Assert.Equal(longDescription, reconstructed);
        }

        using var download = await client.GetAsync("/Services/ExportBackupGeral");

        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Equal(SpreadsheetMediaType, download.Content.Headers.ContentType?.MediaType);
        Assert.EndsWith(".xlsx", download.Content.Headers.ContentDisposition?.FileNameStar, StringComparison.OrdinalIgnoreCase);
        var bytes = await download.Content.ReadAsByteArrayAsync();
        using (var stream = new MemoryStream(bytes))
        using (var workbook = new XLWorkbook(stream))
        {
            var manifest = workbook.Worksheet("_Backup");
            Assert.Equal("InsEmpodera", manifest.Cell("B3").GetString());
            Assert.Equal(2, manifest.Cell("B4").GetValue<int>());
            Assert.NotEmpty(manifest.Cell("B5").GetString());
            var tableNames = new List<string>();
            for (var row = 9; !manifest.Cell(row, 1).IsEmpty(); row++)
                tableNames.Add(manifest.Cell(row, 1).GetString());
            Assert.Equal(countsBefore.Count, tableNames.Count);
            Assert.Equal(countsBefore.Keys.Order(), tableNames.Order());
            Assert.All(tableNames, table => Assert.True(workbook.TryGetWorksheet(table, out _)));
            Assert.Equal(XLWorksheetVisibility.VeryHidden, workbook.Worksheet("_Nulos").Visibility);
            Assert.Equal(XLWorksheetVisibility.VeryHidden, workbook.Worksheet("_TextosLongos").Visibility);
        }

        await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            db.Atores.Remove(await db.Atores.SingleAsync(actor => actor.IdAtores == restoredActorId));
            await db.SaveChangesAsync();
        });

        using var import = await ImportBackupAsync(client, bytes);
        HttpFlowTestSupport.AssertRedirect(import, "/Report");
        using var resultPage = await client.GetAsync("/Report");
        var html = await resultPage.Content.ReadAsStringAsync();
        Assert.Contains("1 registros novos importados", html, StringComparison.Ordinal);
        Assert.Equal(countsBefore, await GetTableCountsAsync());
        var restoredActor = await HttpFlowTestSupport.InDatabaseAsync(_factory, db => db.Atores
            .AsNoTracking()
            .SingleAsync(actor => actor.IdAtores == restoredActorId));
        Assert.Equal(actorName, restoredActor.Nome);
        Assert.Equal(longDescription, restoredActor.PapelSocial1);
        Assert.Null(restoredActor.PapelSocial2);
        Assert.Equal(string.Empty, restoredActor.Telefone);
        Assert.True(restoredActor.DaEquipe);
        Assert.False(restoredActor.Rope);
        Assert.True(restoredActor.Lopiniao);
        Assert.False(restoredActor.Mcomunidade);

        using var secondImport = await ImportBackupAsync(client, bytes);
        HttpFlowTestSupport.AssertRedirect(secondImport, "/Report");
        using var secondResultPage = await client.GetAsync("/Report");
        Assert.Contains(
            "0 registros novos importados",
            await secondResultPage.Content.ReadAsStringAsync(),
            StringComparison.Ordinal);
        Assert.Equal(countsBefore, await GetTableCountsAsync());
    }

    [Fact]
    public async Task BackupEndpoints_RequireAdministratorAndRejectInvalidFilesWithoutDatabaseChanges()
    {
        var viewer = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 4, active: "S");
        using var client = await AuthenticatedClientAsync(viewer);
        var countsBefore = await GetTableCountsAsync();

        using var download = await client.GetAsync("/Services/ExportBackupGeral");
        Assert.Equal(HttpStatusCode.Forbidden, download.StatusCode);

        var token = await HttpFlowTestSupport.GetAntiforgeryTokenAsync(client, "/Account");
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(token), "__RequestVerificationToken");
        form.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("not an Excel backup")), "backupFile", "invalid.xlsx");
        using var import = await client.PostAsync("/Report/ImportarBackup", form);
        Assert.Equal(HttpStatusCode.Forbidden, import.StatusCode);
        Assert.Equal(countsBefore, await GetTableCountsAsync());
    }

    private async Task<HttpClient> AuthenticatedClientAsync(Usuario user)
    {
        var client = HttpFlowTestSupport.CreateClient(_factory);
        using var login = await HttpFlowTestSupport.LoginUsingFormAsync(client, user.Email, HttpFlowTestSupport.ValidPassword);
        HttpFlowTestSupport.AssertRedirect(login, "/");
        return client;
    }

    private static async Task<HttpResponseMessage> ImportBackupAsync(HttpClient client, byte[] bytes)
    {
        var token = await HttpFlowTestSupport.GetAntiforgeryTokenAsync(client, "/Report");
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(token), "__RequestVerificationToken");
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(SpreadsheetMediaType);
        form.Add(file, "backupFile", "roundtrip.xlsx");
        return await client.PostAsync("/Report/ImportarBackup", form);
    }

    private Task<Dictionary<string, int>> GetTableCountsAsync() =>
        HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var connection = db.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open) await connection.OpenAsync();
            foreach (var table in db.Model.GetEntityTypes().Select(entity => entity.GetTableName()).Where(name => name != null).Distinct()!)
            {
                await using var command = connection.CreateCommand();
                command.CommandText = $"SELECT COUNT(*) FROM `{table!.Replace("`", "``", StringComparison.Ordinal)}`";
                result[table] = Convert.ToInt32(await command.ExecuteScalarAsync());
            }
            return result;
        });

    private Task<int> SeedCommunityAsync() =>
        HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var community = new Comunidade
            {
                Nome = $"Comunidade exportável {Guid.NewGuid():N}",
                Local = "Rua da Integração, 10",
                LocalMapa = "Rua da Integração, 10",
                Status = "Em processo",
                Ativo = "S",
                FK_Id_Usuario = 1,
                Dt_Criacao = DateTime.UtcNow,
                Dt_Modificacao = DateTime.UtcNow
            };
            db.Comunidades.Add(community);
            await db.SaveChangesAsync();
            return community.Id_Comunidade;
        });
}
