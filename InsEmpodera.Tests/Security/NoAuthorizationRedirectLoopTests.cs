using System.Net;
using System.Text.RegularExpressions;
using Empodera.Models;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InsEmpodera.Tests.Security;

public sealed class NoAuthorizationRedirectLoopHttpTests : IClassFixture<EmpoderaWebApplicationFactory>
{
    private readonly EmpoderaWebApplicationFactory _factory;

    public NoAuthorizationRedirectLoopHttpTests(EmpoderaWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task AuthenticatedUserDeniedListing_ReceivesForbiddenWithoutRedirectLoopAcrossModules()
    {
        var user = await SeedDeniedUserAsync();
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
            HandleCookies = true,
            BaseAddress = new Uri("https://localhost")
        });
        using var login = await HttpFlowTestSupport.LoginUsingFormAsync(
            client, user.Email, HttpFlowTestSupport.ValidPassword);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        foreach (var path in new[]
                 {
                     "/Users",
                     "/AccessProfile",
                     "/Atores",
                     "/Atividades",
                     "/FichaPrimeiroContato",
                     "/PersonalAssessment",
                     "/PersonalProcess"
                 })
        {
            using var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Null(response.Headers.Location);
        }
    }

    private Task<Usuario> SeedDeniedUserAsync() =>
        HttpFlowTestSupport.InDatabaseAsync(_factory, async db =>
        {
            var modules = new[]
            {
                "Usuarios", "Perfis", "Atores", "Atividades", "Ficha1Contato",
                "AvaliacoesPessoais", "DiariosProcessoPessoal"
            };
            var profile = new Perfil
            {
                Nome = $"Sem listagem {Guid.NewGuid():N}",
                FkIdUsuario = 1,
                DtCriacao = DateTime.UtcNow,
                DtModificacao = DateTime.UtcNow,
                Permissoes = modules.Select(module => new Permissoes
                {
                    Modulo = module,
                    PodeListar = "N",
                    PodeDetalhar = "N",
                    PodeCriar = "N",
                    PodeAtualizar = "N",
                    PodeDeletar = "N"
                }).ToList()
            };
            db.Perfis.Add(profile);
            await db.SaveChangesAsync();

            var user = new Usuario
            {
                Nome = "Usuário sem listagem",
                Email = $"denied-list-{Guid.NewGuid():N}@test.local",
                Senha = HttpFlowTestSupport.ValidPassword,
                Ocupacao = "Teste",
                DtNascimento = new DateTime(1990, 1, 1),
                DtCriacao = DateTime.UtcNow,
                DtAtualizacao = DateTime.UtcNow,
                FkIdPerfil = profile.IdPerfil,
                Ativo = "S"
            };
            user.Senha = new PasswordHasher<Usuario>().HashPassword(user, user.Senha);
            db.Usuarios.Add(user);
            await db.SaveChangesAsync();
            return user;
        });
}

public sealed class NoAuthorizationRedirectLoopArchitectureTests
{
    [Fact]
    public void ListPermissionDenials_DoNotRedirectBackToSameControllerIndex()
    {
        var targets = new Dictionary<string, string>
        {
            ["UsersController.cs"] = "Users",
            ["AccessProfileController.cs"] = "AccessProfile",
            ["AtoresController.cs"] = "Atores",
            ["AtividadesController.cs"] = "Atividades",
            ["FichaPrimeiroContatoController.cs"] = "FichaPrimeiroContato",
            ["PersonalAssessmentController.cs"] = "PersonalAssessment",
            ["PersonalProcessController.cs"] = "PersonalProcess"
        };
        var offenders = new List<string>();

        foreach (var target in targets)
        {
            var source = File.ReadAllText(RepositoryPath("Controllers", target.Key));
            var selfRedirectAfterListDenial =
                @".CanList([^)]*)[sS]{0,180}?RedirectToAction(s*""Index""s*,s*""" +
                Regex.Escape(target.Value) +
                @"""s*)";
            if (Regex.IsMatch(source, selfRedirectAfterListDenial, RegexOptions.CultureInvariant))
                offenders.Add(target.Key);
        }

        Assert.True(
            offenders.Count == 0,
            "Negação de listagem com autorredirecionamento: " + string.Join(", ", offenders));
    }

    private static string RepositoryPath(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "InsEmpodera.csproj")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return segments.Aggregate(directory!.FullName, Path.Combine);
    }
}