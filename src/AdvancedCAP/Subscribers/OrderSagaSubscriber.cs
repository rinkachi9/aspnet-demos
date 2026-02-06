using AdvancedCAP.Data;
using AdvancedCAP.Domain;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;

namespace AdvancedCAP.Subscribers;

public class OrderSagaSubscriber : ICapSubscribe
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderSagaSubscriber> _logger;

    public OrderSagaSubscriber(AppDbContext context, ILogger<OrderSagaSubscriber> logger)
    {
        _context = context;
        _logger = logger;
    }

    [CapSubscribe("payment.succeeded")]
    public async Task HandlePaymentSuccess(dynamic message)
    {
        Guid orderId = Guid.Parse((string)message.OrderId);
        
        // IDEMPOTENCY:
        // We start a transaction. CAP consumer will ONLY ack the message if this transaction commits successsfully.
        using var transaction = _context.Database.BeginTransaction();

        try
        {
             var order = await _context.Orders.FindAsync(orderId);
             if (order != null && order.Status == OrderStatus.Created)
             {
                 order.Status = OrderStatus.Paid;
                 await _context.SaveChangesAsync();
                 _logger.LogInformation("[OrderSaga] Order {OrderId} marked as PAID.", orderId);
             }
             else if (order == null)
             {
                 _logger.LogWarning("[OrderSaga] Order {OrderId} not found!", orderId);
             }

             transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw; // Trigger CAP retry
        }
    }

    [CapSubscribe("payment.failed")]
    public async Task HandlePaymentGenericFailure(dynamic message)
    {
        Guid orderId = Guid.Parse((string)message.OrderId);
        string reason = (string)message.Reason;

        // COMPENSATING TRANSACTION
        using var transaction = _context.Database.BeginTransaction();

        try 
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null && order.Status != OrderStatus.Cancelled)
            {
                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                _logger.LogWarning("[OrderSaga] Order {OrderId} CANCELLED. Reason: {Reason}", orderId, reason);
            }
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}
