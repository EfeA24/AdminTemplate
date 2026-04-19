using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using TrivaWebPage.Data.Connection;
using TrivaWebPage.DependencyInjection;
using TrivaWebPage.Models;
using TrivaWebPage.Routing;
using TrivaWebPage.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap.Add("publicPageSlug", typeof(PublicPageSlugRouteConstraint));
});

builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add(new AuthorizeFilter(
            new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
    })
    .AddRazorRuntimeCompilation()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

// EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Data access / repositories
builder.Services.AddDataAccess(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var log = loggerFactory.CreateLogger("Startup");
    try
    {
        await scope.ServiceProvider.GetRequiredService<CuratorTemplateSeedService>().SeedAsync();
        await scope.ServiceProvider.GetRequiredService<CardDefinitionSeedService>().SeedAsync();
        await scope.ServiceProvider.GetRequiredService<AdminUserSeedService>().SeedAsync();
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Şablon tohumlaması başarısız.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;
    const string legacyPrefix = "/sayfa/";
    if (path.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase))
    {
        var slug = path[legacyPrefix.Length..];
        if (slug.Length > 0 && slug.IndexOf('/') < 0)
        {
            var target = "/" + slug;
            if (context.Request.QueryString.HasValue)
            {
                target += context.Request.QueryString.Value;
            }

            context.Response.Redirect(target, permanent: true);
            return;
        }
    }

    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
        name: "public-site-page",
        pattern: "{slug:publicPageSlug}",
        defaults: new { controller = "SitePage", action = "BySlug" })
    .WithStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
