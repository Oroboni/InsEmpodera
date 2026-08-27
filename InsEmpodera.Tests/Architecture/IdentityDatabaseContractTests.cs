using Xunit;

namespace InsEmpodera.Tests.Architecture;

public sealed class IdentityDatabaseContractTests
{
    [Fact(DisplayName = "Banco — tabela usuarios contém o contrato de segurança do Identity")]
    public void BancoTxt_ContainsIdentitySecurityColumnsAndIndexes()
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "Banco.txt"));

        foreach (var column in new[]
                 {
                     "UserName", "NormalizedUserName", "NormalizedEmail", "EmailConfirmed",
                     "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed",
                     "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount"
                 })
            Assert.Contains($"`{column}`", source, StringComparison.Ordinal);

        Assert.Contains("IX_Usuarios_NormalizedUserName", source, StringComparison.Ordinal);
        Assert.Contains("IX_Usuarios_NormalizedEmail", source, StringComparison.Ordinal);
        Assert.Contains("ADD PRIMARY KEY (`IdUsuario`)", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AUTO_INCREMENT`IdUsuario`", source, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "InsEmpodera.csproj")))
            current = current.Parent;
        return current?.FullName ?? throw new DirectoryNotFoundException("Raiz do projeto não encontrada.");
    }
}
