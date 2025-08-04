using HMS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
//PostGre
builder.Services.AddDbContext<HMSContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("HMSContext") ?? throw new InvalidOperationException("Connection string 'HMSContext' not found.")));
//Sql
//builder.Services.AddDbContext<HMSContext>(dbContextOptions => dbContextOptions.UseSqlServer(builder.Configuration["ConnectionStrings:HMSContext"]));


// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Basic OpenAPI info
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HMS APIs",
        Version = "v1",
        Description = "APIs for HMS",
        Contact = new OpenApiContact
        {
            Name = "NVK",
            Email = "NVK@gmail.com"
        }
    });
    // 🔐 Define JWT Bearer Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\n\nExample: Bearer 12345abcdef"
    });

    // 🔐 Add global security requirement
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    // Enable XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
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
            ValidIssuer = "HMS-eARK",
            ValidAudience = "HMS-eARK",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("3RV5tgb^&*()"))
        };
    });

builder.Services.AddAuthorization();


var app = builder.Build();
// Enable Swagger UI in all environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    options.RoutePrefix = string.Empty; // Serves Swagger at app root (/)
});

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ⬅️ Ensure DB is created and migrations applied
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var context = services.GetRequiredService<HMSContext>();
//    context.Database.Migrate();
//}

app.Run();
