using System.Net;
using ClosedXML.Excel;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class CommunityExportAuthorizationHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private const string SpreadsheetMediaType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly EmpoderaWebApplicationFactory _factory;

    public CommunityExportAuthorizationHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task AnonymousRequest_IsRedirectedWithoutReceivingSpreadsheet()
    {
        var communityId = await SeedCommunityAsync("Comunidade anônima", active: "S");
        using var client = HttpFlowTestSupport.CreateClient(_factory);

        using var response = await client.GetAsync($"/Services/ExportComunidade?id={communityId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account", response.Headers.Location?.OriginalString);
        Assert.NotEqual(SpreadsheetMediaType, response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task AuthenticatedUserWithoutCommunityDetailsPermission_IsDenied()
    {
        var viewer = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 4);
        var communityId = await SeedCommunityAsync("Comunidade restrita", active: "S");
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, viewer.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var response = await client.GetAsync($"/Services/ExportComunidade?id={communityId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(response.Headers.Location);
        Assert.NotEqual(SpreadsheetMediaType, response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task AuthorizedRequest_ForMissingOrInactiveCommunity_ReturnsNotFound()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var inactiveId = await SeedCommunityAsync("Comunidade inativa", active: "N");
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var missing = await client.GetAsync("/Services/ExportComunidade?id=999999");
        using var inactive = await client.GetAsync($"/Services/ExportComunidade?id={inactiveId}");

        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, inactive.StatusCode);
    }

    [Fact]
    public async Task AuthorizedRequest_DownloadsValidXlsxForOnlyRequestedCommunityWithSafeFilename()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var requestedName = "Comunidade" + (char)13 + (char)10 + "../" + (char)34 + $"segura-{Guid.NewGuid():N}";
        var requestedId = await SeedCommunityAsync(requestedName, active: "S");
        var otherName = $"Comunidade não solicitada {Guid.NewGuid():N}";
        await SeedCommunityAsync(otherName, active: "S");
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var response = await client.GetAsync($"/Services/ExportComunidade?id={requestedId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(SpreadsheetMediaType, response.Content.Headers.ContentType?.MediaType);
        var disposition = response.Content.Headers.ContentDisposition;
        Assert.NotNull(disposition);
        var downloadName = disposition!.FileNameStar ?? disposition.FileName ?? string.Empty;
        Assert.EndsWith(".xlsx", downloadName.Trim('"'), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("..", downloadName, StringComparison.Ordinal);
        Assert.DoesNotContain("/", downloadName, StringComparison.Ordinal);
        Assert.DoesNotContain(((char)92).ToString(), downloadName, StringComparison.Ordinal);
        Assert.DoesNotContain(((char)13).ToString(), downloadName, StringComparison.Ordinal);
        Assert.DoesNotContain(((char)10).ToString(), downloadName, StringComparison.Ordinal);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var workbook = new XLWorkbook(stream);
        Assert.Equal(
            requestedName.Replace("\r\n", "\n", StringComparison.Ordinal),
            workbook.Worksheet("Comunidade").Cell(1, 2).GetString()
                .Replace("\r\n", "\n", StringComparison.Ordinal));
        var exportedValues = workbook.Worksheets
            .SelectMany(sheet => sheet.CellsUsed())
            .Select(cell => cell.GetString())
            .ToArray();
        Assert.DoesNotContain(otherName, exportedValues);
    }

    private Task<int> SeedCommunityAsync(string name, string active) =>
        HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var community = new Comunidade
            {
                Nome = name,
                Local = "Rua da exportação, 10, São Paulo",
                LocalMapa = "Rua da exportação, 10, São Paulo",
                Status = "Em diagnóstico",
                Ativo = active,
                FK_Id_Usuario = 1,
                Dt_Criacao = DateTime.UtcNow,
                Dt_Modificacao = DateTime.UtcNow
            };
            db.Comunidades.Add(community);
            await db.SaveChangesAsync();
            return community.Id_Comunidade;
        });
}
