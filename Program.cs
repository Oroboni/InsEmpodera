using Microsoft.EntityFrameworkCore;
using Empodera.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
    .EnableSensitiveDataLogging());

builder.Services.AddSwaggerGen();

builder.Services.AddSession();

builder.Services.AddHttpClient();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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
app.UseStaticFiles();

app.UseSession();

app.UseSwagger();

app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Comerce API v1"));  

app.UseHttpsRedirection();

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
