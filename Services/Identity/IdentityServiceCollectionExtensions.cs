using System.Security.Claims;
using Empodera.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Empodera.Services.Identity;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddEmpoderaIdentity(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services
            .AddIdentityCore<Usuario>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 4;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped<IUserStore<Usuario>, EmpoderaUserStore>();
        services.AddScoped<IUserClaimsPrincipalFactory<Usuario>, EmpoderaClaimsPrincipalFactory>();
        services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromMinutes(30));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        services.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
        {
            options.Cookie.Name = ".Empodera.Identity";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = environment.IsDevelopment() || environment.IsEnvironment("Testing")
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = true;
            options.LoginPath = "/Account";
            options.Events.OnRedirectToLogin = context =>
            {
                if (context.HttpContext.Items.ContainsKey("Empodera.InvalidatedIdentity"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                context.Response.Redirect("/Account");
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
            options.Events.OnValidatePrincipal = async context =>
            {
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<Usuario>>();
                var user = context.Principal is null ? null : await userManager.GetUserAsync(context.Principal);
                var stampClaim = context.Principal?.FindFirstValue(
                    userManager.Options.ClaimsIdentity.SecurityStampClaimType);
                var currentStamp = user is null ? null : await userManager.GetSecurityStampAsync(user);

                if (user is not null &&
                    user.Ativo == "S" &&
                    !string.IsNullOrWhiteSpace(stampClaim) &&
                    string.Equals(stampClaim, currentStamp, StringComparison.Ordinal))
                    return;

                context.HttpContext.Items["Empodera.InvalidatedIdentity"] = true;
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            };
        });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
