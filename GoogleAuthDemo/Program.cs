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

CookiePolicyOptions cookiePolicy = new CookiePolicyOptions() { Secure = CookieSecurePolicy.Always, MinimumSameSitePolicy = SameSiteMode.None };

var builder = WebApplication.CreateBuilder(args);


var configuration = builder.Configuration;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        IndexFormat = "your-index-prefix-{0:yyyy.MM.dd}",
        AutoRegisterTemplate = true
    })
    .CreateLogger();


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


