using AdvancedDi.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SCRUTOR SCANNING & DECORATION ---

// Scan: Auto-register all IRepository implementations as Scoped
builder.Services.Scan(scan => scan
    .FromAssemblyOf<IRepository>()
    .AddClasses(classes => classes.AssignableTo<IRepository>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// Registration: OrderProcessor
builder.Services.AddScoped<IOrderProcessor, OrderProcessor>();

// Decorator: Wrap IOrderProcessor with LoggingDecorator
// This applies to the previously registered IOrderProcessor.
// Order matters: Register Core, then Decorate.
builder.Services.Decorate<IOrderProcessor, LoggingOrderProcessorDecorator>();

// --- 2. KEYED SERVICES (Native .NET 8) ---
builder.Services.AddKeyedScoped<INotificationService, EmailService>("email");
builder.Services.AddKeyedScoped<INotificationService, SmsService>("sms");

// --- 3. CONDITIONAL DI ---
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IPaymentService, DummyPaymentService>();
}
else
{
    builder.Services.AddScoped<IPaymentService, StripePaymentService>();
}

// --- 4. SCOPED BACKGROUND WORKER ---
builder.Services.AddScoped<IScopedProcessingService, DefaultScopedProcessingService>();
builder.Services.AddHostedService<ScopedProcessingWorker>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
