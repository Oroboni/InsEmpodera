using Empodera.Data;
using Empodera.Services.Email;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace InsEmpodera.Tests.Infrastructure;

public sealed class EmpoderaWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection? _sqliteConnection;
    private readonly string? _mysqlConnectionString;
    public CapturingPasswordResetEmailSender PasswordResetEmailSender { get; } = new();

    public EmpoderaWebApplicationFactory()
    {
        var mysqlServer = Environment.GetEnvironmentVariable("TEST_MYSQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(mysqlServer))
        {
            var sqliteDatabaseName = $"insempodera_tests_{Guid.NewGuid():N}";
            _sqliteConnection = new SqliteConnection(
                $"Data Source={sqliteDatabaseName};Mode=Memory;Cache=Shared");
            _sqliteConnection.Open();
            return;
        }

        var databaseName = $"insempodera_tests_{Guid.NewGuid():N}";
        _mysqlConnectionString = $"{mysqlServer.Trim().TrimEnd(';')};Database={databaseName}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        if (_mysqlConnectionString is not null)
        {
            builder.UseSetting("DatabaseProvider", "MySql");
            builder.UseSetting("ConnectionStrings:DefaultConnection", _mysqlConnectionString);
        }
        else
        {
            builder.UseSetting("DatabaseProvider", "Sqlite");
            builder.UseSetting("ConnectionStrings:TestConnection", _sqliteConnection!.ConnectionString);
        }

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
        if (disposing && _mysqlConnectionString is not null)
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
        if (disposing)
            _sqliteConnection?.Dispose();
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
