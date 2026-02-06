using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Infrastructure.Persistence;
using CleanArchitecture.Infrastructure.Persistence.Repositories;
using CleanArchitecture.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddTransient<IEmailService, EmailService>();

        return services;
    }
}
