using System.Globalization;
using LocalizationPatterns;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Localization Services
builder.Services.AddLocalization(); // Looks for resources in default location

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. Configure Supported Cultures
var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("pl-PL")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

// 3. Enable Middleware
app.UseRequestLocalization(localizationOptions);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4. Endpoints
app.MapGet("/welcome", (IStringLocalizer<SharedResource> localizer) =>
{
    // In a real app, these keys "Welcome" would maps to SharedResource.en-US.resx, etc.
    // For this code-first demo without resx files, it returns the key or specific logical strings.
    // To fully demo, we would need to generate .resx files which is complex in this environment.
    // We will simulate the behavior by checking current culture manually for demonstration if resx is missing.
    
    var culture = CultureInfo.CurrentUICulture.Name;
    var message = culture switch 
    {
        "pl-PL" => "Witaj w Świecie .NET!",
        _ => "Welcome to the .NET World!"
    };

    return Results.Ok(new 
    { 
        Culture = culture,
        Message = message,
        LocalizerTest = localizer["Welcome"].Value // This would pull from resx if present
    });
})
.WithOpenApi();

app.Run();
