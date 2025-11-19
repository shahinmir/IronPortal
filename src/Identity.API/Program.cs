using AspNetCoreRateLimit;
using IronExchange.Identity.API.Authorization;
using IronExchange.Identity.API.Configuration;
using IronExchange.Identity.API.Data;
using IronExchange.Identity.API.Models;
using IronExchange.Identity.API.Services;
using IronExchange.ServiceDefaults;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
IdentityModelEventSource.ShowPII = true;
Environment.SetEnvironmentVariable("DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS", "true");
Environment.SetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");

// ====================================
// 🎯 Aspire Service Defaults
// ====================================
builder.AddServiceDefaults();

// ====================================
// 📋 Serilog Configuration
// ====================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Identity.API")
    .WriteTo.Console()
    .WriteTo.File("Logs/identity-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ====================================
// 🎨 MVC & Views
// ====================================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ====================================
// 🗄️ Database Configuration (Aspire)
// ====================================
builder.AddNpgsqlDbContext<ApplicationDbContext>("identitydb");

// Apply database migration automatically
builder.Services.AddMigration<ApplicationDbContext, UsersSeed>();

// ====================================
// 🔐 Identity Configuration
// ====================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ====================================
// 🎫 IdentityServer Configuration
// ====================================
var identityServerBuilder = builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
    options.EmitStaticAudienceClaim = true;
    options.Endpoints.EnablePushedAuthorizationEndpoint = true;

    // ✅ مهم: آدرس Issuer را مشخص کنید
    options.IssuerUri = "https://localhost:7000"; // آدرس Identity Server شما
})
.AddInMemoryIdentityResources(Config.IdentityResources)
.AddInMemoryApiScopes(Config.ApiScopes)
.AddInMemoryApiResources(Config.ApiResources)
.AddInMemoryClients(Config.Clients(builder.Configuration))
.AddAspNetIdentity<ApplicationUser>()
.AddProfileService<ProfileService>()
.AddDeveloperSigningCredential(); // For development only

// ====================================
// 📧 Email Service (SendGrid)
// ====================================
//builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection("SendGrid"));
//builder.Services.AddScoped<IEmailService, SendGridEmailService>();

// ====================================
// 🔒 Permission System
// ====================================
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminPermission", policy =>
        policy.Requirements.Add(new RequirePermissionAttribute("admin.full_access")));

    options.AddPolicy("RequireCatalogWrite", policy =>
        policy.Requirements.Add(new RequirePermissionAttribute("catalog.write")));

    options.AddPolicy("RequireOrderingAccess", policy =>
        policy.Requirements.Add(new RequirePermissionAttribute("ordering.view", "ordering.manage")));
});

// ====================================
// 🛡️ Rate Limiting
// ====================================
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.Configure<ClientRateLimitOptions>(builder.Configuration.GetSection("ClientRateLimiting"));
builder.Services.Configure<ClientRateLimitPolicies>(builder.Configuration.GetSection("ClientRateLimitPolicies"));

builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, AspNetCoreRateLimit.RateLimitConfiguration>();

// ====================================
// 🌐 CORS Configuration (برای Aspire)
// ====================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AspireServices", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ====================================
// 🔑 External Authentication
// ====================================
builder.Services.AddExternalAuthentication(builder.Configuration);

// ====================================
// 🧩 Services Registration
// ====================================
builder.Services.AddTransient<ILoginService<ApplicationUser>, EFLoginService>();
builder.Services.AddTransient<IRedirectService, RedirectService>();

// ====================================
// 🔧 Forwarded Headers (for Aspire Gateway)
// ====================================
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Clear();
});

// ====================================
// 🏗️ Build Application
// ====================================
var app = builder.Build();

// ====================================
// 📡 Map Aspire Health Endpoints
// ====================================
app.MapDefaultEndpoints();

// ====================================
// 🗃️ Database Seeding (Permissions)
// ====================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Wait for database to be ready
        await context.Database.EnsureCreatedAsync();

        // Seed Permissions
        var permissionsSeed = new PermissionsSeed(
            scope.ServiceProvider.GetRequiredService<ILogger<PermissionsSeed>>());
        await permissionsSeed.SeedAsync(context);

        logger.LogInformation("✅ Permissions seeded successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error during permissions seeding");
    }
}

// ====================================
// 🔧 Middleware Pipeline
// ====================================
app.UseForwardedHeaders();

app.UseStaticFiles();

// Cookie policy for Chrome 80+ compatibility
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax
});

// Rate Limiting
app.UseIpRateLimiting();
app.UseClientRateLimiting();

app.UseRouting();

app.UseCors("AspireServices");

// IdentityServer middleware
app.UseIdentityServer();

app.UseAuthorization();

// ====================================
// 🗺️ Endpoints
// ====================================
app.MapDefaultControllerRoute();
app.MapRazorPages();

// ====================================
// ▶️ Run Application
// ====================================
try
{
    Log.Information("🚀 Starting IronExchange Identity Server (Aspire)");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
