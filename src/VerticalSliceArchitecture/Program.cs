using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Domain;
using VerticalSliceArchitecture.Features.Users.ExportData;
using VerticalSliceArchitecture.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Core Services (Infrastructure)
builder.Services.AddScoped<ICloudStorageClient, LocalStorageClient>(); // REAL Storage
builder.Services.AddScoped<IEmailSender, MockEmailSender>(); // Still Mock Email for now

// 2. Database (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention());

// 3. MediatR & Validation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddScoped<VerticalSliceArchitecture.Domain.Services.UserScoringService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // Simple Migration
    
    if (!db.Users.Any())
    {
        db.Users.Add(new User 
        { 
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), 
            Name = "VSA User", 
            Email = "vsa@example.com",
            CreatedAt = DateTime.UtcNow,
            Orders = new List<Order> { new Order { Id = Guid.NewGuid(), Total = 99.99m, Date = DateTime.UtcNow } }
        });
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map Feature Endpoints
ExportUserDataEndpoint.Map(app);
VerticalSliceArchitecture.Features.Users.Create.CreateUserEndpoint.Map(app);
VerticalSliceArchitecture.Features.Users.Get.GetUserEndpoint.Map(app);
VerticalSliceArchitecture.Features.Documents.Upload.UploadDocumentEndpoint.Map(app);

app.Run();
