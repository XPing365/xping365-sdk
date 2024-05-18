using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApp;
using WebApp.Data;

namespace NUnitTestProject.TestSuite;

/// <summary>
/// This class is responsible for the configuration of all services required for WebApp integration tests. In this 
/// example, the ReplaceDbContextWithInMemoryDb method replaces the actual database configuration to use the SQLite 
/// database.
/// </summary>
public sealed class WebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        builder.ConfigureServices(ReplaceDbContextWithInMemoryDb);
    }

    /// <summary>
    /// It is necessary to replace the default DbContext with an in-memory database
    /// </summary>
    private static void ReplaceDbContextWithInMemoryDb(IServiceCollection services)
    {
        var existingDbContextRegistration = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

        if (existingDbContextRegistration != null)
        {
            services.Remove(existingDbContextRegistration);
        }

        var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "sql_lite_tests" };
        var connectionString = connectionStringBuilder.ToString();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
    }

    public ApplicationDbContext CreateDbContext()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>()!;

        dbContext.Database.EnsureCreated();

        return dbContext;
    }
}