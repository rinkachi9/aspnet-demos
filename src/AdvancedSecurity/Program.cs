using AdvancedSecurity.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// 1. Auth Services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
         // For demo, we just accept any token (Simulated)
         // In production, configure Authority, Audience, etc.
         options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
         {
             ValidateIssuer = false, // Demo
             ValidateAudience = false,
             ValidateLifetime = false,
             ValidateIssuerSigningKey = false,
             SignatureValidator = (token, parameters) => new Microsoft.IdentityModel.JsonWebTokens.JsonWebToken(token) 
         };
    });

// 2. Authorization with Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EditPolicy", policy =>
        policy.Requirements.Add(new SameAuthorRequirement()));
});

// 3. Register Handler
builder.Services.AddSingleton<IAuthorizationHandler, DocumentAuthorizationHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "Advanced Security Service Running");

app.Run();
