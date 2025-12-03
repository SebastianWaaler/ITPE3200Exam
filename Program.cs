using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Data.Repositories.Interfaces;
using QuizApp.Data.Repositories.Implementations;
using QuizApp.Models;

/// <summary>
/// Application startup and configuration. Sets up dependency injection, database, authentication,
/// and seeds initial data (roles and default admin user).
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// Configure logging to console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configure SQLite database connection
builder.Services.AddDbContext<QuizContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("QuizContext")));

// Configure ASP.NET Core Identity for user authentication and authorization
// Uses ApplicationUser as the user type and supports roles (Admin, Player)
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<QuizContext>();

// Configure cookie authentication options for development
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Register MVC controllers and Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// SPA: allow our Vue dev server (Vite) to call the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaCors", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173") // SPA dev server origin
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Register repository implementations for dependency injection
// Uses scoped lifetime so each HTTP request gets its own repository instance
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IOptionRepository, OptionRepository>();

var app = builder.Build();

// Seed initial data: create roles and default admin user if they don't exist
// This runs once when the application starts
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Create Admin and Player roles if they don't exist
    string[] roleNames = { "Admin", "Player" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Create default admin user for testing/managing the application
    // Email: admin@example.com, Password: Admin123!
    string adminEmail = "admin@example.com";
    string adminPassword = "Admin123!";

    var adminUser = await userManager.FindByNameAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            foreach (var error in createResult.Errors)
            {
                Console.WriteLine($"Admin user creation error: {error.Code} - {error.Description}");
            }
        }
    }
}

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// SPA: enable CORS (must be between UseRouting and UseAuthorization ideally)
app.UseCors("SpaCors");

// Authentication must come before authorization
app.UseAuthentication();
app.UseAuthorization();

// Map MVC routes - default route goes to Quiz/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Quiz}/{action=Index}/{id?}");

// Map Razor Pages (used for Identity pages like Login, Register)
app.MapRazorPages();

app.Run();