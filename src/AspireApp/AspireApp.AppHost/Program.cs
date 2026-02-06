var builder = DistributedApplication.CreateBuilder(args);

// Resources
var cache = builder.AddRedis("cache");
var postgres = builder.AddPostgres("postgres")
    .AddDatabase("advanced-db");

// API
var api = builder.AddProject<Projects.MinimalApiPipeline>("api")
    .WithReference(postgres)
    .WithReference(cache);

builder.Build().Run();
