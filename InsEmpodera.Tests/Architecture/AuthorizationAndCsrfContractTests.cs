using System.Reflection;
using System.Text.RegularExpressions;
using Empodera.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace InsEmpodera.Tests.Architecture;

public sealed class AuthorizationAndCsrfContractTests
{
    [Fact]
    public void EveryPostActionInApplication_RequiresAntiforgeryValidation()
    {
        var unprotected = typeof(AccountController).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(Controller).IsAssignableFrom(type))
            .SelectMany(type => type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName && !method.IsDefined(typeof(NonActionAttribute)))
                .Where(method => method.IsDefined(typeof(HttpPostAttribute)))
                .Where(method => !method.IsDefined(typeof(ValidateAntiForgeryTokenAttribute)))
                .Select(method => $"{type.Name}.{method.Name}"))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.True(unprotected.Length == 0,
            "Ações POST sem [ValidateAntiForgeryToken]: " + string.Join(", ", unprotected));
    }

    [Fact]
    public void Controllers_DoNotScanUnrelatedModuleDenials()
    {
        var offenders = Directory.GetFiles(RepositoryPath("Controllers"), "*Controller.cs")
            .Select(path => new { Path = path, Source = StripComments(File.ReadAllText(path)) })
            .Where(file => Regex.IsMatch(
                file.Source,
                @"\.Permissoes\s*\.Any\s*\(\s*\w+\s*=>\s*\w+\.Pode(?:Listar|Detalhar|Criar|Atualizar|Deletar)\s*==\s*""N""",
                RegexOptions.CultureInvariant))
            .Select(file => Path.GetFileName(file.Path))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.True(offenders.Length == 0,
            "Controladores com negação global, sem escopo de módulo: " + string.Join(", ", offenders));
    }

    [Fact]
    public void DifferentialControllers_UseCanonicalModules()
    {
        var users = File.ReadAllText(RepositoryPath("Controllers", "UsersController.cs"));
        var profiles = File.ReadAllText(RepositoryPath("Controllers", "AccessProfileController.cs"));
        var process = File.ReadAllText(RepositoryPath("Controllers", "PersonalProcessController.cs"));
        var assessment = File.ReadAllText(RepositoryPath("Controllers", "PersonalAssessmentController.cs"));

        Assert.Contains("p.Modulo == \"Usuarios\"", users, StringComparison.Ordinal);
        Assert.DoesNotContain("p.Modulo == \"Perfis\"", users, StringComparison.Ordinal);
        Assert.Contains("p.Modulo == \"Perfis\"", profiles, StringComparison.Ordinal);
        Assert.Contains("p.Modulo == \"DiariosProcessoPessoal\"", process, StringComparison.Ordinal);
        Assert.DoesNotContain("p.Modulo == \"AvaliacoesPessoais\"", process, StringComparison.Ordinal);
        Assert.Contains("p.Modulo == \"AvaliacoesPessoais\"", assessment, StringComparison.Ordinal);
    }

    private static string StripComments(string source)
    {
        var withoutBlockComments = Regex.Replace(
            source,
            @"/\*.*?\*/",
            string.Empty,
            RegexOptions.Singleline | RegexOptions.CultureInvariant);

        return Regex.Replace(
            withoutBlockComments,
            @"//[^\r\n]*",
            string.Empty,
            RegexOptions.CultureInvariant);
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