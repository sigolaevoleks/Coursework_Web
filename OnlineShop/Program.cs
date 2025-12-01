using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineShop.Data;

var builder = WebApplication.CreateBuilder(args);

// ----------------- Services registration -----------------

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
        options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// ----------------- Build app -----------------

var app = builder.Build();

// ----------------- Seeding roles & admin user -----------------

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 0) убеждаемся, что БД и миграции применены
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        const string adminRoleName = "Admin";
        const string adminEmail = "sigolaev@icloud.com"; // твой email
        const string adminPassword = "Admin123!";        // можешь поменять

        // 1) создаём роль, если её ещё нет
        var roleExists = await roleManager.RoleExistsAsync(adminRoleName);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(adminRoleName));
        }

        // 2) ищем пользователя с указанным email
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        // если аккаунта ещё нет — создаём
        if (adminUser == null)
        {
            var newUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(newUser, adminPassword);

            if (createResult.Succeeded)
            {
                adminUser = await userManager.FindByEmailAsync(adminEmail);
            }
            else
            {
                // если создать не удалось — выходим из сидинга, но не валим приложение
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

        // 3) добавляем пользователя в роль Admin (если он есть)
        if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, adminRoleName))
        {
            await userManager.AddToRoleAsync(adminUser, adminRoleName);
        }
    }
    catch (Exception ex)
    {
        // Логируем, но не даём приложению упасть
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// ----------------- HTTP pipeline -----------------

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // <-- обязательно
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
