using System.Net;
using ClosedXML.Excel;
using Empodera.Controllers;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class PersonalAssessmentValidationHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public PersonalAssessmentValidationHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task Create_WithScoreOutsideOneToFive_ReturnsFormAndDoesNotPersist(int invalidScore)
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var actorId = await SeedActorAsync();
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");
        var before = await CountAsync();

        var fields = ValidFields(actorId, 3);
        fields["CCrimes"] = invalidScore.ToString();
        using var response = await HttpFlowTestSupport.PostFormAsync(
            client,
            "/PersonalAssessment/Create",
            fields,
            tokenPage: $"/PersonalAssessment/Create?atorId={actorId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(before, await CountAsync());
    }

    [Fact]
    public async Task Create_WithUnknownActor_ReturnsFormAndDoesNotPersist()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");
        var before = await CountAsync();

        using var response = await HttpFlowTestSupport.PostFormAsync(
            client,
            "/PersonalAssessment/Create",
            ValidFields(999_999, 3),
            tokenPage: "/PersonalAssessment/Create");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(before, await CountAsync());
    }

    [Fact]
    public async Task Edit_WithInvalidScore_PreservesStoredScoresActorAndAudit()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var target = await SeedAssessmentAsync(score: 2);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        var fields = ValidFields(999_999, 4);
        fields["Saude"] = "6";
        fields["FkIdUsuario"] = "999999";
        fields["DtCriacao"] = "2099-01-01";
        using var response = await HttpFlowTestSupport.PostFormAsync(
            client,
            $"/PersonalAssessment/Edit/{target.AssessmentId}",
            fields,
            tokenPage: $"/PersonalAssessment/Edit/{target.AssessmentId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await HttpFlowTestSupport.InDatabaseAsync(
            _factory,
            db => db.AvaliacaoPessoal.AsNoTracking()
                .SingleAsync(item => item.IdAvaliacao == target.AssessmentId));
        Assert.Equal(2, stored.Saude);
        Assert.Equal(target.ActorId, stored.FK_id_Atores);
        Assert.Equal(1, stored.FkIdUsuario);
        Assert.Equal(target.CreatedAt, stored.DtCriacao);
    }

    private async Task<int> SeedActorAsync() =>
        await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var actor = new Atores
            {
                Nome = $"Ator avaliação {Guid.NewGuid():N}",
                Idade = 30,
                Ativo = "S",
                FkIdUsuario = 1,
                DtCriacao = DateTime.UtcNow,
                DtModificacao = DateTime.UtcNow
            };
            db.Atores.Add(actor);
            await db.SaveChangesAsync();
            return actor.IdAtores;
        });

    private async Task<AssessmentTarget> SeedAssessmentAsync(int score)
    {
        var actorId = await SeedActorAsync();
        return await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var createdAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var assessment = NewAssessment(actorId, score);
            assessment.FkIdUsuario = 1;
            assessment.DtCriacao = createdAt;
            assessment.DtModificacao = createdAt;
            db.AvaliacaoPessoal.Add(assessment);
            await db.SaveChangesAsync();
            return new AssessmentTarget(assessment.IdAvaliacao, actorId, createdAt);
        });
    }

    private Task<int> CountAsync() =>
        HttpFlowTestSupport.InDatabaseAsync(_factory, db => db.AvaliacaoPessoal.CountAsync());

    private static Dictionary<string, string> ValidFields(int actorId, int score) => new()
    {
        ["FK_id_Atores"] = actorId.ToString(),
        ["CCrimes"] = score.ToString(),
        ["Substancias"] = score.ToString(),
        ["Moradia"] = score.ToString(),
        ["Prevencao"] = score.ToString(),
        ["AssBasica"] = score.ToString(),
        ["Educacao"] = score.ToString(),
        ["Saude"] = score.ToString(),
        ["Ocupacao"] = score.ToString(),
        ["Lazer"] = score.ToString()
    };

    private static AvaliacaoPessoal NewAssessment(int actorId, int score) => new()
    {
        FK_id_Atores = actorId,
        CCrimes = score,
        Substancias = score,
        Moradia = score,
        Prevencao = score,
        AssBasica = score,
        Educacao = score,
        Saude = score,
        Ocupacao = score,
        Lazer = score
    };

    private sealed record AssessmentTarget(int AssessmentId, int ActorId, DateTime CreatedAt);
}

public sealed class PersonalAssessmentImportTests : ControllerTestBase
{
    [Fact]
    public async Task SpreadsheetImport_MapsColumnThreeToCrimesAndPersistsOnlyOneToFiveScores()
    {
        var actorName = $"Ator importado {Guid.NewGuid():N}";
        await using var spreadsheet = BuildWorkbook(actorName, crimeScore: 5);
        IFormFile upload = new FormFile(spreadsheet, 0, spreadsheet.Length, "files", "avaliacoes.xlsx")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
        var controller = Attach(new ReportController(Db));

        var result = await controller.RelatorioComunidade(new List<IFormFile> { upload });

        Assert.IsType<RedirectToActionResult>(result);
        var assessment = await Db.AvaliacaoPessoal
            .Include(item => item.Ator)
            .SingleAsync(item => item.Ator.Nome == actorName);
        Assert.Equal(5, assessment.CCrimes);
        var scores = new[]
        {
            assessment.CCrimes, assessment.Substancias, assessment.Moradia,
            assessment.Prevencao, assessment.AssBasica, assessment.Educacao,
            assessment.Saude, assessment.Ocupacao, assessment.Lazer
        };
        Assert.All(scores, score => Assert.InRange(score, 1, 5));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("6")]
    [InlineData("2.5")]
    [InlineData("")]
    public void ImportedScore_RejectsMissingFractionalOrOutOfRangeValues(string value)
    {
        Assert.Throws<InvalidDataException>(
            () => PersonalAssessmentScore.ParseImported(value, "Métrica de teste"));
    }

    private static MemoryStream BuildWorkbook(string actorName, int crimeScore)
    {
        using var workbook = new XLWorkbook();

        var community = workbook.AddWorksheet("Comunidade");
        community.Cell(1, 2).Value = $"Comunidade importada {Guid.NewGuid():N}";
        community.Cell(2, 2).Value = "Rua Importada, 10, São Paulo";
        community.Cell(3, 2).Value = "Descrição";
        community.Cell(4, 2).Value = "Acessível";
        community.Cell(5, 2).Value = "in diagnosis";

        var actors = workbook.AddWorksheet("Atores");
        actors.Cell(1, 1).Value = "Nome";
        actors.Cell(2, 1).Value = actorName;
        actors.Cell(2, 2).Value = "Feminino";
        actors.Cell(2, 3).Value = 30;
        actors.Cell(2, 4).Value = "Liderança";
        actors.Cell(2, 5).Value = "11999990000";
        actors.Cell(2, 6).Value = true;
        actors.Cell(2, 7).Value = false;
        actors.Cell(2, 8).Value = false;
        actors.Cell(2, 9).Value = true;

        foreach (var sheetName in new[] { "Atividades", "Recursos", "Vulnerabilidades", "Diarios", "RSC" })
            workbook.AddWorksheet(sheetName).Cell(1, 1).Value = "Cabeçalho";

        var assessments = workbook.AddWorksheet("Avaliacoes");
        for (var column = 1; column <= 12; column++)
            assessments.Cell(1, column).Value = $"Coluna {column}";
        assessments.Cell(2, 1).Value = actorName;
        assessments.Cell(2, 2).Value = new DateTime(2026, 8, 24);
        assessments.Cell(2, 3).Value = 1;
        assessments.Cell(2, 4).Value = crimeScore;
        assessments.Cell(2, 5).Value = 4;
        assessments.Cell(2, 6).Value = 3;
        assessments.Cell(2, 7).Value = 2;
        assessments.Cell(2, 8).Value = 1;
        assessments.Cell(2, 9).Value = 5;
        assessments.Cell(2, 10).Value = 4;
        assessments.Cell(2, 11).Value = 3;
        assessments.Cell(2, 12).Value = 2;

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}