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
        options.UseSqlite("Data Source=:memory:");
    else
        options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 0, 29)));

    options.EnableSensitiveDataLogging();
});

builder.Services.AddSwaggerGen();

builder.Services.AddSession();

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
    // Public account pages always follow the browser, never a previous user's cookie.
    new CustomRequestCultureProvider(context =>
        context.Request.Path.StartsWithSegments("/Account", StringComparison.OrdinalIgnoreCase)
            ? Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(
                UserCultureService.FromBrowser(context.Request.Headers.AcceptLanguage)))
            : Task.FromResult<ProviderCultureResult?>(null)),
    new CookieRequestCultureProvider(),
    new CustomRequestCultureProvider(context => Task.FromResult<ProviderCultureResult?>(
        new ProviderCultureResult(UserCultureService.FromBrowser(context.Request.Headers.AcceptLanguage))))
];

app.UseRequestLocalization(localizationOptions);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsEnvironment("Testing"))
        await db.Database.EnsureCreatedAsync();
    else
        await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Route data is required to select the RESX catalogue for the current view.
app.UseMiddleware<LocalizedHtmlMiddleware>();

app.UseSession();

app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Comerce API v1"));  

app.UseSwagger();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();

// Exposes the application entry point to the integration-test host.
public partial class Program { }
