using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Unnati.Container;
using Unnati.Helper;
using Unnati.Repos;
using Unnati.Service;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using AspNetCoreRateLimit;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning).Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(@"C:\Users\gourav\source\repos\Unnati\Logs\LogFile.txt")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    //Caching 
    builder.Services.AddResponseCaching();

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddTransient<ICustomerService, CustomerService>();
    builder.Services.AddDbContext<UnnatiContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("sqlDBCon")));

    //Authentication 
    builder.Services.AddAuthentication("BasicAuthentication")
        .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
  
    //Automapper configuration
    var automapper = new MapperConfiguration(item => item.AddProfile(new AutomapperHandler()));
    IMapper mapper = automapper.CreateMapper();
    builder.Services.AddSingleton(mapper);

    //// Add Rate Limiting
    builder.Services.AddOptions();
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
    builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
    builder.Services.AddInMemoryRateLimiting();

    //for Serilog
    builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    });

    builder.Host.UseSerilog();

    //Cors configure
    builder.Services.AddCors(p => p.AddPolicy("corspolicy1", build =>
    {
        build.WithOrigins("https://localhost:7139")
        .AllowAnyMethod()
        .AllowAnyHeader();
    }));

    builder.Services.AddCors(p => p.AddPolicy("corspolicy", build =>
    {
        build.WithOrigins("*").
        AllowAnyMethod().
        AllowAnyHeader();
    }));


    var app = builder.Build();

    // Configure the HTTP request pipeline.
    //if (app.Environment.IsDevelopment())
    //{
        app.UseSwagger();
        app.UseSwaggerUI();
    //}


    app.UseIpRateLimiting();

    app.UseStaticFiles();

    app.UseCors();

    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "There was a problem starting the Application");
    return;
}
finally
{
    Log.CloseAndFlush();
}