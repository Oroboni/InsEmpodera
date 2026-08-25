using Microsoft.EntityFrameworkCore;
using Empodera.Data;
using Empodera.Services;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing"))
        options.UseSqlite(
            builder.Configuration.GetConnectionString("TestConnection")
            ?? "Data Source=Empodera.testing.db");
    else
        options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 0, 29)));

    if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
        options.EnableSensitiveDataLogging();
});

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
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddHttpClient();

builder.Services.AddScoped<RelatorioExcelService>();

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

// Route data is required to select the RESX catalogue for the current view.
app.UseMiddleware<LocalizedHtmlMiddleware>();

app.UseSession();

app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Comerce API v1"));  

app.UseSwagger();


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();

// Exposes the application entry point to the integration-test host.
public partial class Program { }
