using AdvancedMigrations;
using AdvancedMigrations.Services;
using CommandLine;
using DataPersistencePatterns.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: true);
builder.Configuration.AddEnvironmentVariables();

// Services
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connString = builder.Configuration.GetConnectionString("Postgres");
    options.UseNpgsql(connString, npgsql => 
    {
        npgsql.MigrationsAssembly("DataPersistencePatterns");
        npgsql.UseNodaTime();
    })
    .UseSnakeCaseNamingConvention();
});

builder.Services.AddTransient<MigratorService>();
builder.Services.AddTransient<SeederService>();

var host = builder.Build();

// Execute CLI Logic
var parser = Parser.Default;
var result = await parser.ParseArguments<MigrateOptions, RollbackOptions, SeedOptions>(args)
    .MapResult(
        async (MigrateOptions opts) => 
        {
            using var scope = host.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<MigratorService>();
            await service.MigrateAsync(CancellationToken.None);
            return 0;
        },
        async (RollbackOptions opts) => 
        {
            using var scope = host.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<MigratorService>();
            await service.RollbackAsync(opts.TargetMigration, CancellationToken.None);
            return 0;
        },
        async (SeedOptions opts) => 
        {
            using var scope = host.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<SeederService>();
            await service.SeedAsync(opts.Environment, opts.Tag, CancellationToken.None);
            return 0;
        },
        errors => Task.FromResult(1)
    );

return result;
