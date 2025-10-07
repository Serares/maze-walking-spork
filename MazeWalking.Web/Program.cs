using MazeWalking.Web.Extensions;
using MazeWalking.Web.Services;
using MazeWalking.Web.Data;
using MazeWalking.Web.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json;
using MazeWalking.Web.RouteExtensions;

namespace MazeWalking.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create a bootstrap logger for early startup logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            // Configure SQLite database
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=mazewalking.db";

            builder.Services.AddDbContext<GameDataDbContext>(options =>
                options.UseSqlite(connectionString));

            // Register repository and services
            builder.Services.AddTransient<MoveChecker>();
            builder.Services.AddScoped<IGameDataRepository, GameDataRepository>();
            builder.Services.AddScoped<GameEngine>();

            // Add services to the container.
            builder.Services.AddAuthorization();
            // Add Serilog
            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
                    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                    .WriteTo.File("logs/app-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", "MazeWalking.Web");
            });

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            });

            builder.Services.AddCustomCors();
            builder.Services.AddCustomHttpLogging();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Ensure database is created
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GameDataDbContext>();
                dbContext.Database.EnsureCreated();
                Log.Information("Database initialized successfully");
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("ReactApp");

            app.UseHttpLogging();

            // Serve static files from wwwroot (React app)
            app.UseStaticFiles();

            app.UseAuthorization();

            // API routes
            app.MapPosts();

            // SPA fallback - serve index.html for all non-API routes
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
