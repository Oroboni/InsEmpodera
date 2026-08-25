using System.Net;
using System.Text.RegularExpressions;

namespace InsEmpodera.Tests.Infrastructure;

internal static class AuthenticationTestHelper
{
    internal static async Task<HttpResponseMessage> LoginAsync(
        HttpClient client,
        string email,
        string password)
    {
        using var page = await client.GetAsync("/Account");
        page.EnsureSuccessStatusCode();
        var html = await page.Content.ReadAsStringAsync();
        var match = Regex.Match(
            html,
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.IgnoreCase);
        if (!match.Success)
            throw new InvalidOperationException("The login page did not render an antiforgery token.");

        return await client.PostAsync("/Account", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["__RequestVerificationToken"] = WebUtility.HtmlDecode(match.Groups[1].Value)
        }));
    }
}