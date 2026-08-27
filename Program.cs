using Microsoft.EntityFrameworkCore;
using Empodera.Data;
using Empodera.Services;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Empodera.Services.Identity;
using Empodera.Services.Email;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddAntiforgery(options => options.SuppressXFrameOptionsHeader = true);
builder.Services.AddControllersWithViews(options =>
    {
        // Protege automaticamente toda ação insegura atual ou adicionada no futuro.
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    })
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

var loginPermitLimit = builder.Environment.IsEnvironment("Testing") ? 1000 : 10;
var recoveryPermitLimit = builder.Environment.IsEnvironment("Testing") ? 1000 : 5;
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = loginPermitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.AddPolicy("password-recovery", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = recoveryPermitLimit,
            Window = TimeSpan.FromMinutes(15),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing"))
        options.UseSqlite(
            builder.Configuration.GetConnectionString("TestConnection")
            ?? "Data Source=Empodera.testing.db");
    else
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Defina ConnectionStrings__DefaultConnection por variável de ambiente ou cofre de segredos.");
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 29)));
    }

    if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
        options.EnableSensitiveDataLogging();
});

builder.Services.AddEmpoderaIdentity(builder.Environment);
builder.Services.AddGmailPasswordRecovery(builder.Configuration, builder.Environment);

builder.Services.AddSwaggerGen();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        || builder.Environment.IsEnvironment("Testing")
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromHours(4);
});

builder.Services.AddHttpClient();

builder.Services.AddScoped<RelatorioExcelService>();
builder.Services.AddScoped<SpreadsheetExportService>();
builder.Services.AddScoped<ExcelBackupService>();

var app = builder.Build();

var supportedCultures = new[]
{
    new CultureInfo("pt-BR"),
    new CultureInfo("en"),
    new CultureInfo("es")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("pt-BR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

localizationOptions.RequestCultureProviders =
[
    new CookieRequestCultureProvider(),
    new CustomRequestCultureProvider(context => Task.FromResult<ProviderCultureResult?>(
        new ProviderCultureResult(UserCultureService.FromBrowser(context.Request.Headers.AcceptLanguage))))
];

app.UseRequestLocalization(localizationOptions);

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var headers = context.Response.Headers;
        headers.TryAdd("X-Content-Type-Options", "nosniff");
        headers.TryAdd("X-Frame-Options", "DENY");
        headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
        headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
        headers.TryAdd(
            "Content-Security-Policy",
            "default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; " +
            "form-action 'self'; script-src 'self' 'unsafe-inline' https://unpkg.com; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://unpkg.com; " +
            "font-src 'self' data: https://fonts.gstatic.com; img-src 'self' data: https:; " +
            "connect-src 'self' https://nominatim.openstreetmap.org https://geocode.arcgis.com https://viacep.com.br; " +
            "upgrade-insecure-requests");
        return Task.CompletedTask;
    });
    await next();
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsEnvironment("Testing"))
    {
        await db.Database.EnsureCreatedAsync();
        var testPassword = builder.Configuration["Testing:AdminPassword"];
        if (!string.IsNullOrWhiteSpace(testPassword))
        {
            var testUser = await db.Usuarios.FirstOrDefaultAsync(user => user.IdUsuario == 1);
            if (testUser is not null)
            {
                testUser.Senha = new Microsoft.AspNetCore.Identity.PasswordHasher<Empodera.Models.Usuario>()
                    .HashPassword(testUser, testPassword);
                testUser.Ativo = "S";
                await db.SaveChangesAsync();
            }
        }
    }
    else
        await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsEnvironment("Testing"))
    app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();

// Route data is required to select the RESX catalogue for the current view.
app.UseMiddleware<LocalizedHtmlMiddleware>();

app.UseSession();
app.UseAuthentication();
app.UseMiddleware<IdentitySessionBridgeMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "InsEmpodera API v1"));
    app.UseSwagger();
}


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();

// Exposes the application entry point to the integration-test host.
public partial class Program { }
