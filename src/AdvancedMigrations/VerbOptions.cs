using CommandLine;

namespace AdvancedMigrations;

[Verb("migrate", HelpText = "Apply all pending migrations.")]
public class MigrateOptions
{ }

[Verb("rollback", HelpText = "Rollback to a specific target migration.")]
public class RollbackOptions
{
    [Option('t', "target", Required = true, HelpText = "The target migration ID to rollback to (e.g. '20231201120000_InitialCreate').")]
    public string TargetMigration { get; set; } = string.Empty;
}

[Verb("seed", HelpText = "Seed the database with data.")]
public class SeedOptions
{
    [Option('e', "env", Required = false, Default = "Development", HelpText = "Environment to seed data for (Development/Production).")]
    public string Environment { get; set; } = "Development";

    [Option('t', "tag", Required = false, HelpText = "Optional tag to seed specific datasets (e.g. 'performance').")]
    public string? Tag { get; set; }
}
