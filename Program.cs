using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VontaGenta.Data;
using VontaGenta.DependencyInjection;
using VontaGenta.Entities;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();  // Optional: Adds context like request info
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContext");
// ApplicationDbContext is added with Scoped lifetime by default.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

builder.Services.AddAppTimeZonesServices();

builder.Services.AddAppUsersServices();

builder.Services.AddClientsServices();

builder.Services.AddCurrenciesServices();

builder.Services.AddCostCategoriesServices();

builder.Services.AddCostEntriesServices();

builder.Services.AddEmailsServices();

builder.Services.AddHcaptchaServices();

builder.Services.AddOrgsServices();

builder.Services.AddProjectsServices();

builder.Services.AddRevenuesServices();

builder.Services.AddTimesheetsServices();

builder.Services.AddUnitOfWorkServices();

builder.Services.AddAutoMapper(
    cfg => cfg.LicenseKey = builder.Configuration["Licenses:AutoMapper"] ?? throw new InvalidOperationException("AutoMapper license missing"),
    typeof(Program).Assembly
);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Add custom middleware HERE (after auth/session, before MVC)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

Console.WriteLine("Running app now...");

app.Run();

// cSpell:ignore serilog hcaptcha npgsql
