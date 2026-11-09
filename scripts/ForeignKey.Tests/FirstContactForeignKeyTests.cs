using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Xunit;

namespace InsEmpodera.ForeignKey.Tests;

public sealed class FirstContactForeignKeyTests
{
    [Fact(DisplayName = "Banco — FK da ficha para o ator aceita referência válida, rejeita órfão e aplica exclusão em cascata")]
    public async Task FirstContactActorForeignKey_IsEnforcedByMariaDb()
    {
        var serverConnection = Environment.GetEnvironmentVariable("TEST_MYSQL_CONNECTION");
        if (string.IsNullOrWhiteSpace(serverConnection))
            throw new InvalidOperationException("Defina TEST_MYSQL_CONNECTION para executar o teste de FK no MariaDB.");

        var databaseName = $"insempodera_fk_{Guid.NewGuid():N}";
        var connection = new MySqlConnectionStringBuilder(serverConnection) { Database = databaseName };
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(connection.ConnectionString, ServerVersion.AutoDetect(serverConnection))
            .Options;

        await using var db = new ApplicationDbContext(options);
        try
        {
            await db.Database.EnsureCreatedAsync();

            await using (var sqlConnection = new MySqlConnection(connection.ConnectionString))
            {
                await sqlConnection.OpenAsync();
                await using var command = sqlConnection.CreateCommand();
                command.CommandText = """
                    SELECT COUNT(*)
                    FROM information_schema.KEY_COLUMN_USAGE
                    WHERE TABLE_SCHEMA = @database
                      AND TABLE_NAME = 'fichasprimeirocontato'
                      AND COLUMN_NAME = 'IdAtor'
                      AND REFERENCED_TABLE_NAME = 'atores'
                      AND REFERENCED_COLUMN_NAME = 'IdAtor'
                    """;
                command.Parameters.AddWithValue("@database", databaseName);
                Assert.Equal(1L, Convert.ToInt64(await command.ExecuteScalarAsync()));
            }

            var userId = await db.Usuarios.Select(user => user.IdUsuario).FirstAsync();
            var actor = new Atores
            {
                Nome = "Ator do teste de FK",
                FkIdUsuario = userId,
                DtCriacao = DateTime.UtcNow,
                DtModificacao = DateTime.UtcNow
            };
            db.Atores.Add(actor);
            await db.SaveChangesAsync();

            var validForm = NewForm(actor.IdAtores, userId);
            db.FichasPrimeiroContato.Add(validForm);
            await db.SaveChangesAsync();
            Assert.True(await db.FichasPrimeiroContato.AsNoTracking()
                .AnyAsync(form => form.IdFicha == validForm.IdFicha && form.FK_id_Atores == actor.IdAtores));

            // Ids são AUTO_INCREMENT: um valor acima do maior Id existente não aponta para ninguém.
            var missingActorId = await db.Atores.MaxAsync(item => item.IdAtores) + 1000;

            var orphanForm = NewForm(missingActorId, userId);
            db.FichasPrimeiroContato.Add(orphanForm);
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            var mysqlException = Assert.IsType<MySqlException>(exception.InnerException);
            Assert.Equal(1452, mysqlException.Number); // Cannot add or update a child row.

            db.ChangeTracker.Clear();
            Assert.False(await db.FichasPrimeiroContato.AsNoTracking()
                .AnyAsync(form => form.IdFicha == orphanForm.IdFicha));

            // ExecuteDelete ignora o rastreador do EF; só a FK no banco pode remover a ficha.
            Assert.Equal(1, await db.Atores.Where(item => item.IdAtores == actor.IdAtores)
                .ExecuteDeleteAsync());
            Assert.False(await db.FichasPrimeiroContato.AsNoTracking()
                .AnyAsync(form => form.IdFicha == validForm.IdFicha));
        }
        finally
        {
            // Somente a base temporária criada por este teste pode ser apagada.
            if (databaseName.StartsWith("insempodera_fk_", StringComparison.Ordinal))
                await db.Database.EnsureDeletedAsync();
        }
    }

    private static FichaPrimeiroContato NewForm(int actorId, int userId) => new()
    {
        FK_id_Atores = actorId,
        FkIdUsuario = userId,
        DtContato = DateTime.UtcNow,
        HoraContato = TimeSpan.FromHours(9),
        DtCriacao = DateTime.UtcNow,
        DtModificacao = DateTime.UtcNow
    };
}
