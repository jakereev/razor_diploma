using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AlphaMaterials.Data;
using AlphaMaterials.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// 1) DbContext
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) Identity с ролями
builder.Services.AddDefaultIdentity<IdentityUser>(opts =>
    opts.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 3) MVC + глобальная авторизация по умолчанию
builder.Services.AddControllersWithViews(opts =>
{
    var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
    opts.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddRazorPages();

// 4) Ваши сервисы
builder.Services.AddScoped<IReportService, ReportService>();

var app = builder.Build();

// 5) Инициализация БД, ролей и админа
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    context.Database.Migrate();
    await DbInitializer.InitializeAsync(context);

    // Роли
    foreach (var role in new[] { "Admin", "User" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Админ
    const string adminEmail = "admin@alpha.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        var res = await userManager.CreateAsync(admin, "Admin123!");
        if (res.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
        else
            foreach (var e in res.Errors)
                logger.LogError("Admin create error: {Code} {Desc}", e.Code, e.Description);
    }
}

// Настройка конвейера
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
