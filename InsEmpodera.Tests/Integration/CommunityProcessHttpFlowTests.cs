using System.Net;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Integration;

public sealed class CommunityProcessHttpFlowTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public CommunityProcessHttpFlowTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task StartProcess_GetAndMissingTokenDoNotMutate_ValidPostTransitionsCanonicalStatus()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var communityId = await SeedCommunityAsync("Em diagnóstico");
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var getAttempt = await client.GetAsync($"/Comunidade/Processo/{communityId}");
        Assert.Equal(HttpStatusCode.MethodNotAllowed, getAttempt.StatusCode);
        Assert.Equal("Em diagnóstico", await StatusAsync(communityId));

        using var missingToken = await client.PostAsync(
            $"/Comunidade/Processo/{communityId}",
            new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>()));
        Assert.Equal(HttpStatusCode.BadRequest, missingToken.StatusCode);
        Assert.Equal("Em diagnóstico", await StatusAsync(communityId));

        using var validPost = await HttpFlowTestSupport.PostFormAsync(
            client,
            $"/Comunidade/Processo/{communityId}");
        HttpFlowTestSupport.AssertRedirect(validPost, "/Comunidade");
        Assert.Equal("Em processo", await StatusAsync(communityId));
    }

    [Fact]
    public async Task CommunityList_RendersStartPostOnlyForCommunityInDiagnosis()
    {
        var admin = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        var diagnosticId = await SeedCommunityAsync("Em diagnóstico");
        var inProcessId = await SeedCommunityAsync("Em processo");
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, admin.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        using var response = await client.GetAsync("/Comunidade");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains($"/Comunidade/Processo/{diagnosticId}", html, StringComparison.Ordinal);
        Assert.DoesNotContain($"/Comunidade/Processo/{inProcessId}", html, StringComparison.Ordinal);
        Assert.Contains("method=\"post\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("__RequestVerificationToken", html, StringComparison.Ordinal);
        Assert.Contains("Em diagnóstico", html, StringComparison.Ordinal);
        Assert.Contains("Em processo", html, StringComparison.Ordinal);
    }

    private async Task<int> SeedCommunityAsync(string status) =>
        await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var community = new Comunidade
            {
                Nome = $"Comunidade processo {Guid.NewGuid():N}",
                Local = "Rua do fluxo, 10, São Paulo",
                LocalMapa = "Rua do fluxo, 10, São Paulo",
                Status = status,
                Ativo = "S",
                FK_Id_Usuario = 1,
                Dt_Criacao = DateTime.UtcNow,
                Dt_Modificacao = DateTime.UtcNow
            };
            db.Comunidades.Add(community);
            await db.SaveChangesAsync();
            return community.Id_Comunidade;
        });

    private Task<string> StatusAsync(int communityId) =>
        HttpFlowTestSupport.InDatabaseAsync(
            _factory,
            db => db.Comunidades
                .Where(community => community.Id_Comunidade == communityId)
                .Select(community => community.Status)
                .SingleAsync());
}