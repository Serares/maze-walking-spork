using Microsoft.AspNetCore.HttpLogging;

namespace MazeWalking.Web.Extensions
{
    public static class IserviceCollectionExtensions
    {
        public static IServiceCollection AddCustomHttpLogging(
    this IServiceCollection services)
        {
            services.AddHttpLogging(options =>
            {
                // Add the Origin header so it will not be redacted.
                options.RequestHeaders.Add("Origin");

                // Add the rate limiting headers so they will not be redacted.
                options.RequestHeaders.Add("X-Client-Id");
                options.ResponseHeaders.Add("Retry-After");

                // By default, the response body is not included.
                options.LoggingFields = HttpLoggingFields.All;

                // Disable redaction for request and response bodies
                options.MediaTypeOptions.AddText("application/json");
                options.MediaTypeOptions.AddText("text/plain");
                options.MediaTypeOptions.AddText("text/html");

                // Set request body log limit (default is 32KB)
                options.RequestBodyLogLimit = 4096;
                options.ResponseBodyLogLimit = 4096;
            });
            return services;
        }

    public static IServiceCollection AddCustomCors(this IServiceCollection services)
        {
            // Configure CORS for React frontend
            services.AddCors(options =>
            {
                options.AddPolicy("ReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:5173") // Vite dev server
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });
            return services;
        }
    }
}
