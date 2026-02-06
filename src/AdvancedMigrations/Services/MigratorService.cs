using DataPersistencePatterns.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;

namespace AdvancedMigrations.Services;

public class MigratorService(
    AppDbContext dbContext,
    ILogger<MigratorService> logger)
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting database migration...");
        
        var pending = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        if (!pending.Any())
        {
            logger.LogInformation("No pending migrations.");
            return;
        }

        logger.LogInformation("Applying {Count} pending migrations...", pending.Count());
        await dbContext.Database.MigrateAsync(cancellationToken);
        
        logger.LogInformation("Migration completed successfully.");
    }

    public async Task RollbackAsync(string targetMigration, CancellationToken cancellationToken)
    {
        logger.LogInformation("Rolling back database to target: {Target}", targetMigration);

        var migrator = dbContext.GetService<IMigrator>();
        await migrator.MigrateAsync(targetMigration, cancellationToken);
        
        logger.LogInformation("Rollback completed.");
    }
}
