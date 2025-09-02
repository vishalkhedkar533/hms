using CommonLibrary;
using Communication;
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

var builder = WebApplication.CreateBuilder(args);

// Configure CORS to allow only your frontend + Swagger UI
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendAndSwagger", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000"   // React dev server
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // keep only if using cookies/auth headers
    });
});
//"http://localhost:4200",   // Angular dev server
//"https://localhost:5001"   // Swagger UI (adjust to your HTTPS port)
// ----------------------------
// JWT Authentication
// ----------------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var refreshSeconds = builder.Configuration.GetValue<int>("LoggingFilter:RefreshSeconds", 30);
var batchSize = builder.Configuration.GetValue<int>("LoggingFilter:batchSize", 5);
var flushInterval = builder.Configuration.GetValue<int>("LoggingFilter:flushInterval", 5);


// ----------------------------
// Shared logging infrastructure
// ----------------------------
var logChannel = Channel.CreateUnbounded<AppLogEntry>();
var filterState = new AppLogFilterState();
var pgConnection = builder.Configuration.GetConnectionString("HMSContext")!;

// ----------------------------
// Postgres DbContext
// ----------------------------
builder.Services.AddDbContext<HMSContext>(options =>
    options.UseNpgsql(pgConnection));

// ----------------------------
// AutoMapper
// ----------------------------
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AgentProfile>());

// ----------------------------
// HttpContextAccessor
// ----------------------------
builder.Services.AddHttpContextAccessor();

// ----------------------------
// Logging: Clear default, add custom provider
// ----------------------------
builder.Logging.ClearProviders();
builder.Services.AddSingleton(logChannel);
builder.Services.AddSingleton(filterState);

// Register MailTemplateService with ContentRootPath
builder.Services.AddSingleton<FileService>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    return new FileService(env.ContentRootPath);
});
// Register custom logger provider
builder.Services.AddSingleton<ILoggerProvider>(sp =>
    new AppLogLoggerProvider(
        sp.GetRequiredService<Channel<AppLogEntry>>(),
        sp.GetRequiredService<AppLogFilterState>(),
        sp.GetRequiredService<IHttpContextAccessor>()
    ));

// ----------------------------
// Background services
// ----------------------------
// Refresh filter from DB
builder.Services.AddHostedService(sp =>
    new AppLogFilterRefresher(
        sp.GetRequiredService<AppLogFilterState>(),
        pgConnection, refreshSeconds
    )
);

// Flush logs to DB
builder.Services.AddHostedService(sp =>
    new AppLogBackgroundService(
        sp.GetRequiredService<Channel<AppLogEntry>>(),
        pgConnection, batchSize, flushInterval
    )
);

// ----------------------------
// Controllers
// ----------------------------
builder.Services.AddControllers();

// ----------------------------
// Swagger / OpenAPI
// ----------------------------
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
// Rate Limiting
// ----------------------------
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("PerUser", httpContext =>
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? httpContext.Connection.RemoteIpAddress?.ToString()
                     ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromSeconds(10),
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

// ----------------------------
// Middleware
// ----------------------------
app.UseMiddleware<WhitelistHeadersMiddleware>();
app.UseHttpsRedirection();
app.UseCors("FrontendAndSwagger");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

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
// Controllers
// ----------------------------
app.MapControllers().RequireRateLimiting("PerUser");

// ----------------------------
// Test logging
// ----------------------------
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started and custom logger initialized.");
logger.LogError("Test error to ensure background service picks it up.");

// ----------------------------
// Run app
// ----------------------------
await app.RunAsync();
