using System.Reflection;
using Elastic.Apm.AspNetCore;
using Elastic.Apm.NetCoreAll;
using GoogleAuthDemo.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using Nest;
using Serilog.Formatting.Elasticsearch;
using Amazon.Runtime;
using Elastic.CommonSchema.Serilog;
using Elastic.Apm.SerilogEnricher;
using Microsoft.Extensions.Logging;
using GoogleAuthDemo.Services;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var host = builder.WebHost;

host.UseUrls("http://*:5178");

builder.Services.AddScoped<B2CUsersService>();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentUserName()
    .Enrich.WithExceptionDetails()
    .Enrich.WithProperty("ApplicationName", $"{Assembly.GetEntryAssembly()?.GetName().Name} - {builder.Configuration["DOTNET_ENVIRONMENT"]}")
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["ElasticsearchSettings:uri"]))
    {
        CustomFormatter = new EcsTextFormatter(),
        AutoRegisterTemplate = true,
        IndexFormat = "indexlogs",
        ModifyConnectionSettings = x => x.BasicAuthentication(configuration["ElasticsearchSettings:username"], configuration["ElasticsearchSettings:password"])
    })
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Host.UseSerilog(Log.Logger, true);

// Add ElasticSearch client
var elasticSearchUri = new Uri(configuration["ElasticsearchSettings:uri"]);
var elasticSearchUsername = configuration["ElasticsearchSettings:username"];
var elasticSearchPassword = configuration["ElasticsearchSettings:password"];

builder.Services.AddSingleton<IElasticClient>(new ElasticClient(new ConnectionSettings(elasticSearchUri)
    .DefaultIndex("logs")
    .BasicAuthentication(elasticSearchUsername, elasticSearchPassword)));

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

builder.Services.AddMicrosoftIdentityWebAppAuthentication(configuration, "AzureAdB2C");

builder.Services.AddMvc(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

builder.Services.AddControllersWithViews();

// Additional services configuration
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
    options.HandleSameSiteCookieCompatibility();
});

var app = builder.Build();
app.UseCookiePolicy(new CookiePolicyOptions()
{
    Secure = CookieSecurePolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.None
});

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseAllElasticApm(configuration);
    // //  app.UseDeveloperExceptionPage();
    // IdentityModelEventSource.ShowPII = true;
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
app.UseMetricServer();
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");


    // Requer autenticação para outras rotas
    endpoints.MapControllerRoute(
        name: "secure",
        pattern: "{controller}/{action}/{id?}",
        defaults: null,
        constraints: null,
        dataTokens: new { RequiresAuthentication = true });

        
    endpoints.MapMetrics();

    endpoints.MapControllerRoute(
        name: "calculator",
        pattern: "{controller=Calculator}/{action=Index}");
});

app.MapRazorPages();
app.Run();
