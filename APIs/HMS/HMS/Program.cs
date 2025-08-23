using HMS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models.Mapping;
using Serilog;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// Postgres DbContext
// ----------------------------
builder.Services.AddDbContext<HMSContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HMSContext")
        ?? throw new InvalidOperationException("Connection string 'HMSContext' not found.")));

// ----------------------------
// Serilog
// ----------------------------
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Logging.ClearProviders();
// Replace this line:
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AgentProfile>());
builder.Logging.AddSerilog(logger);

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
    options.IncludeXmlComments(xmlPath);
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
// Register MenuAuthorizeFilter for DI
// ----------------------------
//builder.Services.AddScoped<MenuAuthorizeFilter>();

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HMS API V1");
    c.RoutePrefix = string.Empty;
});

// Middleware
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
