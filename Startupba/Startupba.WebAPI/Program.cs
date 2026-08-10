using Startupba.Services.Database;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Startupba.WebAPI.Filters;
using Startupba.Services.Services;
using Startupba.Services.Interfaces;
using Startupba.Services.Helpers;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DotNetEnv;

// Load .env file if it exists (for local development)
// In Docker, environment variables are provided by docker-compose
try
{
    var possibleEnvPaths = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), ".env"),
        Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"),
        Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env"),
    };

    bool envLoaded = false;
    foreach (var envPath in possibleEnvPaths)
    {
        if (File.Exists(envPath))
        {
            Env.Load(envPath);
            envLoaded = true;
            break;
        }
    }

    if (!envLoaded)
    {
        Env.Load();
    }
}
catch (FileNotFoundException)
{
    // .env file not found - this is OK in Docker environments
    // Environment variables will be provided by docker-compose
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IGenderService, GenderService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IStartupStatusService, StartupStatusService>();
builder.Services.AddScoped<IStartupService, StartupService>();
builder.Services.AddScoped<IStartupImageService, StartupImageService>();
builder.Services.AddScoped<IDonationService, DonationService>();
builder.Services.AddScoped<IBlogPostService, BlogPostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IPlatformSettingService, PlatformSettingService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IUserAnalyticsService, UserAnalyticsService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();


// Configure database
// Try to get connection string from configuration first (Docker sets this via environment)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// If not found or empty (local development), use Trusted_Connection (Windows Auth)
if (string.IsNullOrWhiteSpace(connectionString))
{
    var sqlServer = Environment.GetEnvironmentVariable("SQL__SERVER") ?? ".";
    var sqlDatabase = Environment.GetEnvironmentVariable("SQL__DATABASE") ?? "StartupbaDb";

    connectionString = $"Server={sqlServer};Database={sqlDatabase};TrustServerCertificate=True;Trusted_Connection=True;";
}

builder.Services.AddDatabaseServices(connectionString);

// Add configuration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddMapster();

var jwtKey = Environment.GetEnvironmentVariable("JWT__KEY")
    ?? builder.Configuration["JWT:KEY"]
    ?? throw new InvalidOperationException("JWT__KEY is required.");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT__ISSUER")
    ?? builder.Configuration["JWT:ISSUER"]
    ?? "Startupba";
var jwtAudience = Environment.GetEnvironmentVariable("JWT__AUDIENCE")
    ?? builder.Configuration["JWT:AUDIENCE"]
    ?? "Startupba";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddControllers(x =>
    {
        x.Filters.Add<ExceptionFilter>();
    }
);

var corsOrigins = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "http://localhost:5130",
    "https://localhost:5130",
    "http://127.0.0.1:5130",
};
var corsEnv = Environment.GetEnvironmentVariable("CORS__ORIGINS")
    ?? builder.Configuration["CORS:ORIGINS"];
if (!string.IsNullOrWhiteSpace(corsEnv))
{
    foreach (var origin in corsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        corsOrigins.Add(origin);
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("StartupbaCors", policy =>
    {
        policy.WithOrigins(corsOrigins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Za dodavanje opisnog teksta pored swagger call-a
var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

builder.Services.AddSwaggerGen(c =>
{
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only redirect HTTP to HTTPS when not in Development (allows mobile app / emulator to use http://10.0.2.2:5130)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("StartupbaCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<StartupbaDbContext>();

    var pendingMigrations = dataContext.Database.GetPendingMigrations().Any();

    if (pendingMigrations)
    {
        dataContext.Database.Migrate();
    }

    DataSeeder.SeedImageFiles(dataContext);

    // Note: the recommendation system is content-based (category filtering) and
    // computed on demand in StartupService - no model training is required.
}

app.Run();
