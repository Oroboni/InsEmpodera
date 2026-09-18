using Empodera.Data;
using Empodera.Services.Email;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace InsEmpodera.Tests.Infrastructure;

public sealed class EmpoderaWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _mysqlConnectionString;
    private readonly string _databaseName = $"insempodera_http_{Guid.NewGuid():N}";
    public CapturingPasswordResetEmailSender PasswordResetEmailSender { get; } = new();

    public EmpoderaWebApplicationFactory()
    {
        var mysqlServer = Environment.GetEnvironmentVariable("TEST_MYSQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(mysqlServer))
            throw new InvalidOperationException("Defina TEST_MYSQL_CONNECTION para executar integrações HTTP em MySQL.");
        var connectionBuilder = new MySqlConnectionStringBuilder(mysqlServer) { Database = _databaseName };
        _mysqlConnectionString = connectionBuilder.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DatabaseProvider", "MySql");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _mysqlConnectionString);

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
            // Falhas reais continuam aparecendo nas asserções e no TRX. O log
            // detalhado do EF tornava o resumo ilegível até em testes aprovados.
            logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IPasswordResetEmailSender>();
            services.AddSingleton<IPasswordResetEmailSender>(PasswordResetEmailSender);
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _databaseName.StartsWith("insempodera_http_", StringComparison.Ordinal))
        {
            try
            {
                using var scope = Services.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                database.Database.EnsureDeleted();
            }
            catch (InvalidOperationException)
            {
                // A inicialização pode ter falhado antes da criação do banco temporário.
            }
        }

        base.Dispose(disposing);
    }
}

public sealed class CapturingPasswordResetEmailSender : IPasswordResetEmailSender
{
    private readonly ConcurrentQueue<CapturedPasswordResetEmail> _messages = new();

    public IReadOnlyList<CapturedPasswordResetEmail> Messages => _messages.ToArray();

    public bool TryQueue(string recipientEmail, string resetUrl)
    {
        _messages.Enqueue(new CapturedPasswordResetEmail(recipientEmail, resetUrl));
        return true;
    }

    public void Clear()
    {
        while (_messages.TryDequeue(out _))
        {
        }
    }
}

public sealed record CapturedPasswordResetEmail(string RecipientEmail, string ResetUrl);
