using System.Reflection;
using Empodera.Controllers;
using InsEmpodera.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace InsEmpodera.Tests.Architecture;

public sealed class CrudContractTests
{
    private static readonly Type[] CrudControllers =
    {
        typeof(UsersController), typeof(AccessProfileController), typeof(ComunidadeController),
        typeof(AtoresController), typeof(AtividadesController), typeof(DiarioCampoController),
        typeof(FichaPrimeiroContatoController), typeof(PersonalAssessmentController),
        typeof(PersonalProcessController)
    };

    [Fact]
    public void EveryCrudPost_IsProtectedAgainstCrossSiteRequestForgery()
    {
        var unprotected = CrudControllers
            .SelectMany(type => ActionMethods(type).Select(method => (type, method)))
            .Where(item => item.method.IsDefined(typeof(HttpPostAttribute)))
            .Where(item => !item.method.IsDefined(typeof(ValidateAntiForgeryTokenAttribute)))
            .Select(item => $"{item.type.Name}.{item.method.Name}")
            .ToArray();

        Assert.True(unprotected.Length == 0,
            "POST actions without [ValidateAntiForgeryToken]: " + string.Join(", ", unprotected));
    }

    [Fact]
    public void CrudControllers_HaveNoAmbiguousPostRoutes()
    {
        var ambiguous = CrudControllers
            .SelectMany(type => ActionMethods(type)
                .Where(method => method.IsDefined(typeof(HttpPostAttribute)))
                .GroupBy(ActionName, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => $"{type.Name}.{group.Key} ({group.Count()} overloads)"))
            .ToArray();

        Assert.True(ambiguous.Length == 0,
            "Ambiguous POST routes: " + string.Join(", ", ambiguous));
    }

    [Theory]
    [MemberData(nameof(CompleteCrudControllers))]
    public void CompleteCrud_HasCreateReadUpdateAndDeleteActions(Type controllerType)
    {
        var methods = ActionMethods(controllerType).ToArray();
        Assert.Contains(methods, method => ActionName(method).StartsWith("Create", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(methods, method => ActionName(method) is "Index" or "Details" or "Edit" or "ComunidadesDetalhes");
        Assert.Contains(methods, method => ActionName(method).StartsWith("Edit", StringComparison.OrdinalIgnoreCase) || ActionName(method) == "ComunidadesDetalhes");
        Assert.Contains(methods, method => ActionName(method).StartsWith("Delete", StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<object[]> CompleteCrudControllers()
    {
        yield return new object[] { typeof(UsersController) };
        yield return new object[] { typeof(ComunidadeController) };
        yield return new object[] { typeof(AtoresController) };
        yield return new object[] { typeof(AtividadesController) };
        yield return new object[] { typeof(DiarioCampoController) };
        yield return new object[] { typeof(FichaPrimeiroContatoController) };
        yield return new object[] { typeof(PersonalAssessmentController) };
    }

    private static IEnumerable<MethodInfo> ActionMethods(Type controllerType) =>
        controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName && !method.IsDefined(typeof(NonActionAttribute)));

    private static string ActionName(MethodInfo method) =>
        method.GetCustomAttribute<ActionNameAttribute>()?.Name ?? method.Name;
}
