using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using SecurityPatterns.Auth;
using SecurityPatterns.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Secrets Management (Azure Key Vault)
// Only add if configured (e.g. in Production)
var keyVaultUrl = builder.Configuration["KeyVault:Url"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}

// 2. Authentication: JWT + Certificate (mTLS)
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:5001"; // Mock Identity Provider
        options.Audience = "security-api";
        options.RequireHttpsMetadata = false; // Dev only
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Strict expiry
        };
    })
    .AddCertificate(options =>
    {
        // mTLS Logic
        options.AllowedCertificateTypes = CertificateTypes.All;
        options.RevocationMode = X509RevocationMode.NoCheck; // For demo (Self-signed)
        options.Events = new CertificateAuthenticationEvents
        {
            OnCertificateValidated = context =>
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, context.ClientCertificate.Subject, ClaimValueTypes.String, context.Options.ClaimsIssuer),
                    new Claim(ClaimTypes.Name, context.ClientCertificate.Subject, ClaimValueTypes.String, context.Options.ClaimsIssuer)
                };
                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                context.Success();
                return Task.CompletedTask;
            }
        };
    });

// 3. Authorization: Permissions
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustHaveAdminPermission", policy =>
        policy.Requirements.Add(new PermissionRequirement("admin_access")));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Security Middleware Pipeline
app.UseSecurityHeaders(); // 1. Headers first

// 2. Secure Static Files
var staticFileOptions = new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Enforce Hard Security Headers for Static Assets
        var headers = ctx.Context.Response.Headers;
        
        // Cache Control: Immutable (for fingerprintable assets)
        headers.Append("Cache-Control", "public,max-age=31536000,immutable");
        
        // Anti-MIME-Sniffing (Critical for user uploaded content)
        headers.Append("X-Content-Type-Options", "nosniff");
        
        // CSP is valid here too if we want to restrict what acts as a script
        // headers.Append("Content-Security-Policy", "script-src 'none'");
    },
    // Whitelist: Only serve from known safe folders if needed (default is wwwroot)
    // RequestPath = "/static"
};
app.UseStaticFiles(staticFileOptions);

// 3. IP Filtering (SafeList)
// Note: In real setup, usually done at WAF/Ingress level, but good as defense-in-depth
// app.UseMiddleware<IpSafelistMiddleware>(); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
