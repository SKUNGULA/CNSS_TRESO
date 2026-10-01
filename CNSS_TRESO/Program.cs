using CNSS_TRESO.Application.Security;
using CNSS_TRESO.Common;
using CNSS_TRESO.Infrastructure.DependencyInjection;
using CNSS_TRESO.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Auth/Login");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Privacy");
    options.Conventions.AllowAnonymousToPage("/Error");
});
builder.Services.AddAntiforgery(options =>
    options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICallerContextAccessor, HttpCallerContextAccessor>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Login?handler=Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.Cookie.Name = isDevelopment ? "CnssTreso" : "__Host-CnssTreso";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.IsEssential = true;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.AdministratorOnly, policy =>
        policy.RequireAuthenticatedUser().RequireRole("Administrator"));
});

builder.Services.AddCleanArchitectureServices(builder.Configuration);

builder.Services.PostConfigure<CNSS_TRESO.Application.Configuration.ReportBranding>(opt =>
{
    if (string.IsNullOrWhiteSpace(opt.LogoPath))
    {
        var root = builder.Environment.WebRootPath ?? builder.Environment.ContentRootPath;
        foreach (var name in new[] { "logo.png", "logo.jpg", "logo.jpeg" })
        {
            var candidate = System.IO.Path.Combine(root, "img", name);
            if (System.IO.File.Exists(candidate)) { opt.LogoPath = candidate; break; }
        }
    }
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), geolocation=(), microphone=()");
    await next();
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
