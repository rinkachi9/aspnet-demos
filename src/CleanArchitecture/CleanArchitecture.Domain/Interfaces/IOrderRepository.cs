using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task AddAsync(Order order);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IEmailService
{
    Task SendOrderConfirmationAsync(Order order);
}
