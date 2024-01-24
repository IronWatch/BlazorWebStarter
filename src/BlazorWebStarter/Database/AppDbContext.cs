using Microsoft.EntityFrameworkCore;
using Npgsql;
using BlazorWebStarter.Services;

namespace BlazorWebStarter.Database;

public class AppDbContext(
    AppEnvConfiguration configuration,
    ILogger<AppDbContext> logger)
    : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        NpgsqlConnectionStringBuilder csb = new();
        csb.Host = configuration.DatabaseHostname;
        csb.Port = configuration.DatabasePort;
        csb.Database = configuration.DatabaseDbName;
        csb.Username = configuration.DatabaseUsername;
        csb.Password = configuration.DatabasePassword;
        csb.Pooling = true;
        csb.MaxPoolSize = 50;

        optionsBuilder.UseNpgsql(csb.ToString());
    }

    public async Task SeedDatabase(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Database newly created. Processing Database Seed Data");

        // Seed here

        await this.SaveChangesAsync(cancellationToken);
    }

    public static async Task AppStartup(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using AppDbContext db = await services
            .GetRequiredService<IDbContextFactory<AppDbContext>>()
            .CreateDbContextAsync(cancellationToken);

        bool noMigrationsInDb = !(await db.Database.GetAppliedMigrationsAsync(cancellationToken)).Any();

        await db.Database.MigrateAsync(cancellationToken);

        if (noMigrationsInDb && db.Database.GetMigrations().Any())
        {
            await db.SeedDatabase(cancellationToken);
        }
    }

}
