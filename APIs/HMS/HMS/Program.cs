using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HMS.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<HMSContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HMSContext") ?? throw new InvalidOperationException("Connection string 'HMSContext' not found.")));

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
