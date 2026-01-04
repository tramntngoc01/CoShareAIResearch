using CoShare.Domain.Auth;
using CoShare.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CoShare.Api.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration testing.
/// Uses in-memory database and mock services.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            services.RemoveAll<DbContextOptions<AuthDbContext>>();
            services.RemoveAll<AuthDbContext>();

            // Add in-memory database
            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
            });

            // Configure test Auth options
            services.Configure<AuthOptions>(options =>
            {
                options.OtpTtlSeconds = 120;
                options.OtpMaxRequestsPerWindow = 3;
                options.OtpRateLimitWindowSeconds = 300;
                options.OtpMaxVerificationAttempts = 5;
                options.OtpLength = 6;
                options.AccessTokenExpirySeconds = 3600;
                options.RefreshTokenExpiryDays = 30;
                options.JwtSigningKey = "TestSecretKeyThatIsAtLeast32CharactersLong!!";
                options.JwtIssuer = "CoShareTest";
                options.JwtAudience = "CoShareTest";
            });

            // Ensure database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            db.Database.EnsureCreated();
        });

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });

        builder.UseEnvironment("Development");
    }
}
