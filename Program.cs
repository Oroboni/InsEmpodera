var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Rota para homepage (dashboard após login)
app.MapControllerRoute(
    name: "homepage",
    pattern: "homepage",
    defaults: new { controller = "Home", action = "HomePage" });

// Rota alternativa para home
app.MapControllerRoute(
    name: "home-alt",
    pattern: "home",
    defaults: new { controller = "Home", action = "HomePage" });

// Rotas específicas da sidebar (só acessíveis após login)
app.MapControllerRoute(
    name: "comunidades",
    pattern: "comunidades",
    defaults: new { controller = "Home", action = "Comunidades" });

app.MapControllerRoute(
    name: "atores",
    pattern: "atores",
    defaults: new { controller = "Home", action = "Atores" });

// ... outras rotas da sidebar ...

// Rota padrão (Index = Login)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Fallback para aplicação logada
app.MapFallbackToController("HomePage", "Home");

app.Run();