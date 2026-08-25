using System.Net;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Security;

public sealed class InactiveUserPermissionUnitTests
{
    [Theory]
    [InlineData("N")]
    [InlineData("?")]
    [InlineData("")]
    [InlineData(null)]
    public void PermissionGrants_NeverAuthorizeUserWithoutExactActiveStatus(string? activeStatus)
    {
        var user = new Usuario
        {
            Ativo = activeStatus!,
            Perfil = new Perfil
            {
                Permissoes = new List<Permissoes>
                {
                    new()
                    {
                        Modulo = "Atores",
                        PodeListar = "S",
                        PodeDetalhar = "S",
                        PodeCriar = "S",
                        PodeAtualizar = "S",
                        PodeDeletar = "S"
                    }
                }
            }
        };

        Assert.False(user.CanList("Atores"));
        Assert.False(user.CanViewDetails("Atores"));
        Assert.False(user.CanCreate("Atores"));
        Assert.False(user.CanUpdate("Atores"));
        Assert.False(user.CanDelete("Atores"));
    }
}

public sealed class InactiveSessionAuthorizationHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public InactiveSessionAuthorizationHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Theory]
    [InlineData("N")]
    [InlineData("?")]
    public async Task ExistingSession_LosesModuleAccessAfterUserStopsBeingExactlyActive(string status)
    {
        var user = await HttpFlowTestSupport.SeedUserAsync(_factory, profileId: 1);
        using var client = HttpFlowTestSupport.CreateClient(_factory);
        using (var login = await HttpFlowTestSupport.LoginUsingFormAsync(
                   client, user.Email, HttpFlowTestSupport.ValidPassword))
            HttpFlowTestSupport.AssertRedirect(login, "/");

        await HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var persisted = await db.Usuarios.SingleAsync(item => item.IdUsuario == user.IdUsuario);
            persisted.Ativo = status;
            await db.SaveChangesAsync();
        });

        using var response = await client.GetAsync("/Atores");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }
}
