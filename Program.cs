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

// Rota padrão → abre no Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 🔹 Alias /homepage → HomePage
app.MapControllerRoute(
    name: "homepage",
    pattern: "homepage",
    defaults: new { controller = "Home", action = "HomePage" });

// 🔹 Alias /home → HomePage
app.MapControllerRoute(
    name: "home-alt",
    pattern: "home",
    defaults: new { controller = "Home", action = "HomePage" });

// 🔹 Fallback → se rota não existir, cai no HomePage
app.MapFallbackToController("HomePage", "Home");

app.Run();
