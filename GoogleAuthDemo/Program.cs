using System.Reflection;
using GoogleAuthDemo.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

CookiePolicyOptions cookiePolicy = new CookiePolicyOptions() { Secure = CookieSecurePolicy.Always, MinimumSameSitePolicy = SameSiteMode.None };

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// configureLogging();
// builder.Host.UseSerilog();

var app = builder.Build();
app.UseCookiePolicy(cookiePolicy);
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
app.UseHttpMetrics();


app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    endpoints.MapMetrics();

    endpoints.MapControllerRoute(
        name: "calculator",
        pattern: "{controller=calculator}/{action=Index}");
});

app.MapRazorPages();

app.Run();


// void configureLogging()
// {
//     var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

//     var configuration = new ConfigurationBuilder()
//         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//         .AddJsonFile($"appsettings.{environment}.json", optional: true).Build();

//         Log.Logger = new LoggerConfiguration()
//         .Enrich.FromLogContext()
//         .Enrich.WithExceptionDetails()
//         .WriteTo.Debug()
//         .WriteTo.Console() 
//         .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
//         .Enrich.WithProperty("Environment", environment)
//         .ReadFrom.Configuration(configuration)
//         .CreateLogger();
// }

// ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment){
//     return new ElasticsearchSinkOptions (new Uri(configuration["ElasticConfiguration:Uri"])){
//         AutoRegisterTemplate = true,
//         IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".","-")}-{environment.ToLower()}-{DateTime.UtcNow:yyyy-MM}",
//         NumberOfReplicas = 1,
//         NumberOfShards = 2,

//     };
// }
