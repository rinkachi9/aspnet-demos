using AdvancedCAP.Data;
using AdvancedCAP.Subscribers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. CAP Configuration
builder.Services.AddCap(x =>
{
    // Outbox Storage (PostgreSQL)
    x.UseEntityFramework<AppDbContext>(); 
    // Uses the DbContext's connection string and transaction management
    // Ensure "cap" schema exists or tables are created (x.UsePostgreSql() would also work but UseEntityFramework is better for Transaction integration)

    // Transport (RabbitMQ)
    x.UseRabbitMQ(o =>
    {
        o.HostName = "localhost";
        o.UserName = "guest";
        o.Password = "guest";
    });

    // Dashboard UI (/cap)
    x.UseDashboard(); 
});

// Register Subscribers (Consumers)
builder.Services.AddTransient<OrderSubscriber>(); 
builder.Services.AddTransient<PaymentSubscriber>();
builder.Services.AddTransient<OrderSagaSubscriber>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-migrate database (for demo ease)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // Creates 'Orders' table AND 'cap.published'/'cap.received' tables
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
