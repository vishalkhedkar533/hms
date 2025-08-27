using CommonLibrary;
using HMS.Data;
using HMS.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models.Mapping;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Channels;
using System.Threading.RateLimiting;
using static HMS.Logging.AppLogLogger;

var builder = WebApplication.CreateBuilder(args);
// Load configs automatically
// - appsettings.json
// - appsettings.{Environment}.json
// - environment variables
// - command line args
var logChannel = Channel.CreateUnbounded<AppLogEntry>();
var filterState = new AppLogFilterState();
var pgConnection = builder.Configuration.GetConnectionString("HMSContext")!;

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
// ----------------------------
// Postgres DbContext
// ----------------------------
builder.Services.AddDbContext<HMSContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HMSContext")
        ?? throw new InvalidOperationException("Connection string 'HMSContext' not found.")));

// ----------------------------
// Serilog
// ----------------------------
//var logger = new LoggerConfiguration()
//    .ReadFrom.Configuration(builder.Configuration)
//    .Enrich.FromLogContext()
//    .CreateLogger();
builder.Logging.ClearProviders();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AgentProfile>());
//builder.Logging.AddSerilog(logger);
// Register HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Logger provider with automatic UserId/UserName capture
builder.Services.AddSingleton<ILoggerProvider>(sp =>
    new AppLogLoggerProvider(logChannel, filterState, sp.GetRequiredService<IHttpContextAccessor>()));

// Background services
builder.Services.AddHostedService(sp => new AppLogBackgroundService(logChannel, pgConnection));
builder.Services.AddHostedService(sp => new AppLogFilterRefresher(filterState, pgConnection));
builder.Services.AddHostedService(sp =>
    new AppLogBackgroundService(
        sp.GetRequiredService<Channel<AppLogEntry>>(),
        pgConnection
    )
);
// ----------------------------
// Controllers
// ----------------------------
builder.Services.AddControllers();

// ----------------------------
// Swagger / OpenAPI
// ----------------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HMS APIs",
        Version = "v1",
        Description = "APIs for HMS",
        Contact = new OpenApiContact { Name = "NVK", Email = "NVK@gmail.com" }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// ----------------------------
// JWT Authentication
// ----------------------------
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "super_secret_key"))
        };
    });

builder.Services.AddAuthorization();

// ----------------------------
// Per-User Rate Limiting (Partitioned by JWT Claim)
// ----------------------------
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("PerUser", httpContext =>
    {
        // Use NameIdentifier (mapped to "sub" in JWT) if available, else IP
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? httpContext.Connection.RemoteIpAddress?.ToString()
                     ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,                    // max 5 requests
            Window = TimeSpan.FromSeconds(10),  // per 10s
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Try again later.", token);
    };
});

var app = builder.Build();
app.UseMiddleware<WhitelistHeadersMiddleware>();
app.Use(async (context, next) =>
{
    await next();

    var allowedResponseHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Content-Type",
        //"Authorization", // rarely needed but kept if token passthrough is required
        //"Cache-Control",
        //"Pragma",
        //"Expires",
        //"Content-Length",
        "Transfer-Encoding",
        "Date",
        "Server",
        "Access-Control-Allow-Origin",
        "Access-Control-Allow-Headers",
        "Access-Control-Allow-Methods",
        "Access-Control-Expose-Headers"
    };

    var headersToRemove = context.Response.Headers
        .Where(h => !allowedResponseHeaders.Contains(h.Key))
        .Select(h => h.Key)
        .ToList();

    foreach (var header in headersToRemove)
    {
        context.Response.Headers.Remove(header);
    }
});


// ----------------------------
// Swagger
// ----------------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HMS API V1");
    c.RoutePrefix = string.Empty;
});

// ----------------------------
// Middleware
// ----------------------------
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// ----------------------------
// Controllers
// ----------------------------
app.MapControllers().RequireRateLimiting("PerUser");

await app.RunAsync();