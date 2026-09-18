using Empodera.Data;
using Empodera.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Xunit;

namespace InsEmpodera.Tests.Infrastructure;

public abstract class ControllerTestBase : IAsyncLifetime
{
    private readonly string _databaseName = $"insempodera_unit_{Guid.NewGuid():N}";

    protected ApplicationDbContext Db { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var mysqlServer = Environment.GetEnvironmentVariable("TEST_MYSQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(mysqlServer))
            throw new InvalidOperationException("Defina TEST_MYSQL_CONNECTION para executar os testes de banco em MySQL.");
        var serverVersion = ServerVersion.AutoDetect(mysqlServer);
        var connectionBuilder = new MySqlConnectionStringBuilder(mysqlServer) { Database = _databaseName };
        Db = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseMySql(connectionBuilder.ConnectionString, serverVersion)
                .Options);
        await Db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (Db is not null)
        {
            if (_databaseName.StartsWith("insempodera_unit_", StringComparison.Ordinal))
                await Db.Database.EnsureDeletedAsync();
            await Db.DisposeAsync();
        }
    }

    protected T Attach<T>(T controller, int? userId = 1, IFormCollection? form = null)
        where T : Controller
    {
        var context = new DefaultHttpContext { Session = new TestSession() };
        if (userId.HasValue)
        {
            context.Session.SetString("Email", $"user{userId}@test.local");
            context.Session.SetString("ID", userId.Value.ToString());
        }

        if (form is not null)
            context.Request.Form = form;

        controller.ControllerContext = new ControllerContext { HttpContext = context };
        return controller;
    }

    protected async Task<Comunidade> CreateCommunityAsync(string? name = null)
    {
        var community = new Comunidade
        {
            Nome = name ?? $"Comunidade {Guid.NewGuid():N}",
            Local = "Rua de Teste, 123, São Paulo",
            LocalMapa = "Rua de Teste, 123, São Paulo",
            Status = "Em processo",
            Ativo = "S",
            Dt_Criacao = DateTime.UtcNow,
            Dt_Modificacao = DateTime.UtcNow,
            FK_Id_Usuario = 1
        };
        Db.Comunidades.Add(community);
        await Db.SaveChangesAsync();
        return community;
    }

    protected async Task<Atores> CreateActorAsync(Comunidade? community = null, string? name = null)
    {
        var actor = new Atores
        {
            Nome = name ?? $"Ator {Guid.NewGuid():N}",
            Idade = 30,
            Ativo = "S",
            DtCriacao = DateTime.UtcNow,
            DtModificacao = DateTime.UtcNow,
            FkIdUsuario = 1
        };
        Db.Atores.Add(actor);
        await Db.SaveChangesAsync();

        if (community is not null)
        {
            Db.AtorComunidades.Add(new AtorComunidade
            {
                FK_id_Atores = actor.IdAtores,
                FkIdComunidade = community.Id_Comunidade
            });
            await Db.SaveChangesAsync();
        }

        return actor;
    }

    protected async Task<Eixo> CreateAxisAsync(string? name = null)
    {
        var axis = new Eixo { Nome = name ?? $"Eixo {Guid.NewGuid():N}" };
        Db.Eixos.Add(axis);
        await Db.SaveChangesAsync();
        return axis;
    }
}
