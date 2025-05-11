using System;
using CampusNavigation.Data;
using CampusNavigation.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options => {
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(
        connectionString, 
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions => {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }
    );
});

// Register database seed service
builder.Services.AddHostedService<DatabaseSeedService>();

// Register shutdown cleanup service
builder.Services.AddHostedService<ShutdownCleanupService>();

// Register DatabaseService for IDatabaseService
builder.Services.AddScoped<IDatabaseService, DatabaseService>();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Configure static files
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure routing
app.UseRouting();

// Map controllers and SPA fallback
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
