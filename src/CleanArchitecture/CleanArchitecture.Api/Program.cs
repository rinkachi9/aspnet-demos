using Asp.Versioning;
using CleanArchitecture.Api.Endpoints;
using CleanArchitecture.Application;
using CleanArchitecture.Infrastructure;
using CleanArchitecture.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. API Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

// 3. Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 4. Endpoints
var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

app.MapOrderEndpoints(apiVersionSet);

// Ensure DB Created (For demo purposes)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
