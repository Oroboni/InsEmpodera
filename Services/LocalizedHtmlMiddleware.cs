using System.Globalization;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace Empodera.Services;

/// <summary>
/// Localizes legacy Razor output using the resource file for the current view plus
/// the small global and dynamic catalogues.
/// </summary>
public sealed partial class LocalizedHtmlMiddleware(RequestDelegate next)
{
    private const string ResourcePrefix = "InsEmpodera.Resources.";
    private static readonly ConcurrentDictionary<string, IReadOnlyList<ResourceEntry>> EntryCache = new();

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await next(context);

            if (CultureInfo.CurrentUICulture.Name.Equals("pt-BR", StringComparison.OrdinalIgnoreCase)
                || context.Response.ContentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) != true)
            {
                buffer.Position = 0;
                await buffer.CopyToAsync(originalBody);
                return;
            }

            buffer.Position = 0;
            using var reader = new StreamReader(buffer, Encoding.UTF8);
            var html = await reader.ReadToEndAsync();
            var resourceNames = GetResourceNames(
                context.Request.RouteValues["controller"]?.ToString(),
                context.Request.RouteValues["action"]?.ToString());
            var protectedBlocks = new List<string>();
            html = ProtectedBlockRegex().Replace(html, match =>
            {
                var token = $"___I18N_PROTECTED_{protectedBlocks.Count}___";
                protectedBlocks.Add(match.Value);
                return token;
            });
            html = TextNodeRegex().Replace(html, match =>
                $">{Translate(match.Groups[1].Value, CultureInfo.CurrentUICulture, resourceNames)}<");
            html = LocalizableAttributeRegex().Replace(html, match =>
                $"{match.Groups[1].Value}=\"{Translate(match.Groups[2].Value, CultureInfo.CurrentUICulture, resourceNames)}\"");
            for (var index = 0; index < protectedBlocks.Count; index++)
                html = html.Replace($"___I18N_PROTECTED_{index}___", protectedBlocks[index], StringComparison.Ordinal);

            var bytes = Encoding.UTF8.GetBytes(html);
            context.Response.ContentLength = bytes.Length;
            await originalBody.WriteAsync(bytes);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private static string Translate(string text, CultureInfo culture, IReadOnlyList<string> resourceNames)
    {
        foreach (var entry in GetEntries(resourceNames))
        {
            if (!entry.Pattern.IsMatch(text)) continue;
            var translation = entry.Resources.GetString(entry.Key, culture);
            if (!string.IsNullOrWhiteSpace(translation))
                text = entry.Pattern.Replace(text, translation);
        }

        return text;
    }

    public static IReadOnlyDictionary<string, string> GetCatalog(CultureInfo culture, string? pagePath)
    {
        var pathParts = (pagePath ?? string.Empty).Split('/', StringSplitOptions.RemoveEmptyEntries);
        var controller = pathParts.ElementAtOrDefault(0);
        var action = pathParts.ElementAtOrDefault(1) ?? "index";

        return GetEntries(GetResourceNames(controller, action))
            .GroupBy(entry => entry.Source, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(entry => entry.Resources.GetString(entry.Key, culture))
                    .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? group.Key,
                StringComparer.Ordinal);
    }

    private static IReadOnlyList<string> GetResourceNames(string? controller, string? action)
    {
        var names = new List<string>();
        if (!string.IsNullOrWhiteSpace(controller) && !string.IsNullOrWhiteSpace(action))
            names.Add($"{ResourcePrefix}Controllers.{controller}Controller");

        names.Add($"{ResourcePrefix}SharedResource");
        return names;
    }

    private static IReadOnlyList<ResourceEntry> GetEntries(IReadOnlyList<string> resourceNames) =>
        resourceNames.SelectMany(resourceName => EntryCache.GetOrAdd(resourceName, LoadEntries))
            .GroupBy(entry => entry.Source, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderByDescending(entry => entry.Source.Length)
            .ToArray();

    private static IReadOnlyList<ResourceEntry> LoadEntries(string resourceName)
    {
        var manager = new ResourceManager(resourceName, typeof(SharedResource).Assembly);
        ResourceSet? set;
        try
        {
            set = manager.GetResourceSet(new CultureInfo("pt-BR"), true, true);
        }
        catch (MissingManifestResourceException)
        {
            return Array.Empty<ResourceEntry>();
        }

        if (set is null) return Array.Empty<ResourceEntry>();

        return set.Cast<System.Collections.DictionaryEntry>()
            .Select(entry => new ResourceEntry(
                entry.Value?.ToString() ?? "",
                entry.Key.ToString()!,
                manager,
                CreateFlexiblePattern(entry.Value?.ToString() ?? "")))
            .Where(entry => entry.Source.Length >= 2)
            .ToArray();
    }

    private static Regex CreateFlexiblePattern(string source)
    {
        var parts = Regex.Split(source.Trim(), @"\s+")
            .Where(part => part.Length > 0)
            .Select(Regex.Escape);
        return new Regex(string.Join(@"\s+", parts), RegexOptions.Compiled);
    }

    private sealed record ResourceEntry(
        string Source,
        string Key,
        ResourceManager Resources,
        Regex Pattern);

    [GeneratedRegex(@">([^<>]+)<", RegexOptions.Compiled)]
    private static partial Regex TextNodeRegex();

    [GeneratedRegex(@"<(script|style|textarea)\b[^>]*>.*?</\1>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ProtectedBlockRegex();

    [GeneratedRegex("\\b(placeholder|title|aria-label|alt|data-tooltip)=\\\"([^\\\"]+)\\\"", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex LocalizableAttributeRegex();
}
