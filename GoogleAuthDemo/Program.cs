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


CookiePolicyOptions cookiePolicy = new CookiePolicyOptions() { Secure = CookieSecurePolicy.Always, MinimumSameSitePolicy = SameSiteMode.None };

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var host = builder.WebHost;

host.UseUrls("http://*:8000");


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

// // Adicione a configura��o para o servi�o IElasticClient esta mal !!!!!!!!!
// var elasticSearchUri = new Uri(configuration["ElasticsearchSettings:uri"]);
// builder.Services.AddSingleton<IElasticClient>(new ElasticClient(new ConnectionSettings(elasticSearchUri)));

// // Adicione a configuração para o serviço IElasticClient
var elasticSearchUri = new Uri(configuration["ElasticsearchSettings:uri"]);
var defaultIndex = "logs"; // Defina o nome do índice padrão aqui
var settings = new ConnectionSettings(elasticSearchUri)
    .DefaultIndex(defaultIndex) // Defina o índice padrão aqui
    .BasicAuthentication(configuration["ElasticsearchSettings:username"], configuration["ElasticsearchSettings:password"]);

builder.Services.AddSingleton<IElasticClient>(new ElasticClient(settings));


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

var app = builder.Build();
app.UseCookiePolicy(cookiePolicy);

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseAllElasticApm(configuration);
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

    endpoints.MapMetrics();

    endpoints.MapControllerRoute(
        name: "calculator",
        pattern: "{controller=calculator}/{action=Index}");
});


app.MapRazorPages();
app.Run();


