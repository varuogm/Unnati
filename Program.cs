using Serilog;
using AutoMapper;
using System.Text;
using Unnati.Helper;
using Unnati.Repos;
using Unnati.Service;
using Unnati.Models;
using Serilog.Events;
using Unnati.Container;
using AspNetCoreRateLimit;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog.Sinks.Seq;
using QuestPDF.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning).Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(@"C:\Users\gourav\source\repos\Unnati\Logs\LogFile.txt")
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    QuestPDF.Settings.License = LicenseType.Community;

    //Caching 
    builder.Services.AddResponseCaching();

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    //Swagger
    builder.Services.AddSwaggerGen(item =>
    {
        item.SwaggerDoc("v1", new OpenApiInfo { Title = "Unnati", Version = "v1" });
        item.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });

        item.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="Bearer"
                    }
                },
                new string[]{}
            }
        });
        });

    //Register 
    builder.Services.AddTransient<ICustomerService, CustomerService>();
    builder.Services.AddTransient<IRefreshHandler, RefreshHandler>();
    builder.Services.AddTransient<IUserService, UserService>();
    builder.Services.AddTransient<IEmailService, EmailService>();
    builder.Services.AddScoped<IPDFGeneratorService, PDFGeneratorService>();
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

    //Database connection
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
    builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

    builder.Host.UseSerilog();

    //JWT Auth
    var _authkey = builder.Configuration.GetValue<string>("JwtSettings:securitykey");
    var _jwtsetting = builder.Configuration.GetSection("JwtSettings");
    builder.Services.Configure<JwtSettings>(_jwtsetting);

    builder.Services.AddAuthentication(item =>
    {
        item.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        item.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(item =>
    {
        item.RequireHttpsMetadata = true;
        item.SaveToken = true;
        item.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authkey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
        // explicit forbiddon response
        item.Events = new JwtBearerEvents
        {
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return context.Response.WriteAsync("You are not authorized to access this resource.");
            }
        };
    });

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

    //SEQ dashbaord setup
    var _seqSettings = builder.Configuration.GetSection("Seq");
    builder.Services.AddLogging(item =>
    {
        item.AddSeq(_seqSettings);
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    //if (app.Environment.IsDevelopment())
    //{
    app.UseSwagger();
    app.UseSwaggerUI();
    //}


    
    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseCors();

    app.UseIpRateLimiting();

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