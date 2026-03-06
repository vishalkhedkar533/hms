using AutoMapper;
using CommonLibrary;
using HMS.Caching;
using HMS.Controllers;
using HMS.Data;
using HMS.Logging;
using HMS.MapperProfiles;
using HMS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models.DB;
using Models.Mapping;
using Npgsql;
using System.Reflection;
using System.Text;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

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
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AgentProfile>();
    cfg.AddProfile<AuditTrailProfile>();
    cfg.AddProfile<CommissionMgmtProfile>();
    cfg.AddProfile<LocationMappingProfile>();
    cfg.AddProfile<UiFieldsMappingProfile>();
    cfg.AddProfile<BranchMasterProfile>();
    cfg.AddProfile<InboxProfile>();
    cfg.AddProfile<SrApproverProfile>();
    cfg.AddProfile<PartnerBranchHierarchyProfile>();
    cfg.AddProfile<UserProfile>();
    cfg.AddProfile<ChannelBranchHierarchyProfile>();
    cfg.AddProfile<DesignationMasterProfile>();
});

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
// Database Connections
// ----------------------------
// PostgreSQL
builder.Services.AddScoped<NpgsqlConnection>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new NpgsqlConnection(config.GetConnectionString("HMSContext"));
});

// Oracle
//builder.Services.AddScoped<OracleConnection>(sp =>
//{
//    var config = sp.GetRequiredService<IConfiguration>();
//    return new OracleConnection(config.GetConnectionString("OracleContext"));
//});

// SQL Server
//builder.Services.AddScoped<SqlConnection>(sp =>
//{
//    var config = sp.GetRequiredService<IConfiguration>();
//    return new SqlConnection(config.GetConnectionString("SqlServerContext"));
//});

// ----------------------------
// Services
// ----------------------------
builder.Services.AddHttpClient();
builder.Services.AddScoped<GenericCacheService>();
builder.Services.AddScoped<DatabaseService>(); // ✅ new consolidated service
builder.Services.AddScoped<IAuthClaimService, AuthClaimService>();
// ----------------------------
// Background services
// ----------------------------
builder.Services.AddHostedService(sp =>
    new AppLogFilterRefresher(
        sp.GetRequiredService<AppLogFilterState>(),
        pgConnection, refreshSeconds
    )
);

builder.Services.AddHostedService(sp =>
    new AppLogBackgroundService(
        sp.GetRequiredService<Channel<AppLogEntry>>(),
        pgConnection, batchSize, flushInterval
    )
);
builder.Services.AddHostedService<CacheRefreshBackgroundService>();

// ----------------------------
// Controllers
// ----------------------------
builder.Services.AddControllers();
builder.Services.AddScoped<AgentController>();

// Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedSites",
        policy =>
        {
            policy.WithOrigins(
                builder.Configuration.GetValue<string>("CORS:AllowedSites", string.Empty).Split(';')
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
});

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "super_secret_key"))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ----------------------------
// Rate Limiting
// ----------------------------
//builder.Services.AddRateLimiter(options =>
//{
//    options.AddPolicy("RateLimitPerUser", httpContext =>
//    {
//        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
//                     ?? httpContext.Connection.RemoteIpAddress?.ToString()
//                     ?? "anonymous";

//        return RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
//        {
//            PermitLimit = 5,
//            Window = TimeSpan.FromSeconds(10),
//            QueueLimit = 0,
//            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
//        });
//    });

//    options.OnRejected = async (context, token) =>
//    {
//        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
//        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Try again later.", token);
//    };
//});
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
    mapper.ConfigurationProvider.AssertConfigurationIsValid(); // Throws if any map missing
}
// ----------------------------
// Middleware
// ----------------------------
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowedSites");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<WhitelistHeadersMiddleware>();
//app.UseRateLimiter();

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
app.MapControllers().RequireRateLimiting("RateLimitPerUser");

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
