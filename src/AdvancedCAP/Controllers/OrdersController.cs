using AdvancedCAP.Data;
using AdvancedCAP.Domain;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedCAP.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICapPublisher _publisher;

    public OrdersController(AppDbContext context, ICapPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(string product, int quantity)
    {
        // 1. Start Transaction
        using var transaction = _context.Database.BeginTransaction(_publisher, autoCommit: false);

        try
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                Product = product,
                Quantity = quantity,
                CreatedAt = DateTime.UtcNow
            };

            // 2. Business Logic (Save to DB)
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 3. Publish Event (Outbox) - CAP writes to "cap.published" table within SAME transaction
            await _publisher.PublishAsync("order.created", new { order.Id, order.Product });

            // 4. Commit (DB + Outbox saved atomically)
            await transaction.CommitAsync();

            return Ok(new { order.Id, Status = "Published to Outbox" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
}
