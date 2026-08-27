using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using Microsoft.AspNetCore.Identity;
using Empodera.Services;
using Empodera.Services.Email;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Empodera.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly IPasswordResetEmailSender _passwordResetEmailSender;
    private readonly IOptions<GmailSmtpOptions> _emailOptions;
    private readonly IWebHostEnvironment _environment;

    public AccountController(
        ILogger<AccountController> logger,
        UserManager<Usuario> userManager,
        SignInManager<Usuario> signInManager,
        IPasswordResetEmailSender passwordResetEmailSender,
        IOptions<GmailSmtpOptions> emailOptions,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
        _passwordResetEmailSender = passwordResetEmailSender;
        _emailOptions = emailOptions;
        _environment = environment;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Index(string Email, string Password)
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ViewData["LoginError"] = "Informe o e-mail e a senha.";
            return View();
        }

        var user = await _userManager.FindByEmailAsync(Email.Trim());
        if (user is null || user.Ativo != "S")
        {
            ViewData["LoginError"] = "E-mail ou senha inválidos.";
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            Password,
            isPersistent: false,
            lockoutOnFailure: true);
        if (result.Succeeded)
        {
            if (!UserCultureService.HasSavedMode(Request))
                UserCultureService.FollowBrowser(Response);
            return RedirectToAction("Index", "Home");
        }

        ViewData["LoginError"] = result.IsLockedOut
            ? "Acesso temporariamente bloqueado após várias tentativas. Tente novamente mais tarde."
            : "E-mail ou senha inválidos.";
        return View();
    }

    [AllowAnonymous]
    public IActionResult Forgot()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("password-recovery")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        var responseTimer = Stopwatch.StartNew();

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user is not null && user.Ativo == "S")
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var relativePath = Url.Action(
                    nameof(ResetPassword),
                    "Account",
                    new { email = user.Email, code = encodedToken });
                var publicBaseUri = ResolvePublicBaseUri();

                if (relativePath is not null && publicBaseUri is not null)
                {
                    var resetUrl = new Uri(publicBaseUri, relativePath).AbsoluteUri;
                    if (!_passwordResetEmailSender.TryQueue(user.Email, resetUrl))
                        _logger.LogError("A fila de recuperação de senha está indisponível ou cheia.");
                }
                else
                {
                    _logger.LogError(
                        "A URL pública de recuperação não está configurada. Defina Email__PublicBaseUrl com HTTPS.");
                }
            }

            await PadRecoveryResponseAsync(responseTimer);
            ModelState.Clear();
            ViewData["RecoverySubmitted"] = true;
            return View("Forgot", new ForgotPasswordViewModel());
        }

        await PadRecoveryResponseAsync(responseTimer);
        return View("Forgot", model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? email, string? code)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            return View(new ResetPasswordViewModel());

        return View(new ResetPasswordViewModel
        {
            Email = email,
            Code = code
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user is null || user.Ativo != "S")
        {
            AddInvalidResetLinkError();
            return View(model);
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
        }
        catch (FormatException)
        {
            AddInvalidResetLinkError();
            return View(model);
        }

        var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
        if (result.Succeeded)
            return RedirectToAction(nameof(ResetPasswordConfirmation));

        if (result.Errors.Any(error => error.Code == "InvalidToken"))
            AddInvalidResetLinkError();
        else
            foreach (var error in result.Errors)
                ModelState.AddModelError(nameof(model.Password), TranslateIdentityError(error));

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Account");
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private Uri? ResolvePublicBaseUri()
    {
        var configured = _emailOptions.Value.PublicBaseUrl?.Trim();
        if (!string.IsNullOrWhiteSpace(configured) &&
            Uri.TryCreate(configured, UriKind.Absolute, out var configuredUri) &&
            configuredUri.Scheme == Uri.UriSchemeHttps)
            return new Uri(configuredUri.AbsoluteUri.TrimEnd('/') + "/");

        if (!_environment.IsDevelopment() && !_environment.IsEnvironment("Testing"))
            return null;

        var requestBase = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
        return Uri.TryCreate(requestBase, UriKind.Absolute, out var requestUri) &&
               requestUri.Scheme == Uri.UriSchemeHttps
            ? requestUri
            : null;
    }

    private static async Task PadRecoveryResponseAsync(Stopwatch timer)
    {
        var remaining = TimeSpan.FromMilliseconds(300) - timer.Elapsed;
        if (remaining > TimeSpan.Zero)
            await Task.Delay(remaining);
    }

    private void AddInvalidResetLinkError() => ModelState.AddModelError(
        string.Empty,
        "Este link de recuperação é inválido, já foi utilizado ou expirou. Solicite um novo link.");

    private static string TranslateIdentityError(IdentityError error) => error.Code switch
    {
        "PasswordTooShort" => "A senha deve ter pelo menos 8 caracteres.",
        "PasswordRequiresDigit" => "A senha deve conter pelo menos um número.",
        "PasswordRequiresLower" => "A senha deve conter pelo menos uma letra minúscula.",
        "PasswordRequiresUpper" => "A senha deve conter pelo menos uma letra maiúscula.",
        "PasswordRequiresNonAlphanumeric" => "A senha deve conter pelo menos um caractere especial.",
        "PasswordRequiresUniqueChars" => "A senha deve conter pelo menos 4 caracteres diferentes.",
        _ => "Não foi possível redefinir a senha. Verifique os dados e solicite um novo link."
    };
}
