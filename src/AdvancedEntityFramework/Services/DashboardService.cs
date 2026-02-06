using AdvancedEntityFramework.Data;
using AdvancedEntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEntityFramework.Services;

public record DashboardDto(List<Order> RecentOrders, List<SystemLog> RecentLogs, UserStats? Stats);
public record UserStats(int OrderCount, string Username);

public class DashboardService(IDbContextFactory<OptimizationDbContext> contextFactory)
{
    public async Task<DashboardDto> GetDashboardAsync(int userId)
    {
        // 1. Start the tasks (The queries start executing immediately upon invocation)
        // Each method creates its OWN DbContext, allowing parallel execution on DB 
        // (assuming connection pool allows it and DB can handle concurrency)
        var ordersTask = GetOrdersAsync(userId);
        var logsTask = GetSystemLogsAsync();
        var statsTask = GetUserStatsAsync(userId);

        // 2. Wait for all to complete
        // This runs roughly in MAX(Time_Orders, Time_Logs, Time_Stats) instead of SUM(...)
        await Task.WhenAll(ordersTask, logsTask, statsTask);

        // 3. Return results
        return new DashboardDto(
            await ordersTask,
            await logsTask,
            await statsTask
        );
    }

    private async Task<List<Order>> GetOrdersAsync(int userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        
        // Simulating slow query
        // await Task.Delay(300); 

        return await context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Include(o => o.Items)
            .ToListAsync();
    }

    private async Task<List<SystemLog>> GetSystemLogsAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        
        // Simulating slow query
        // await Task.Delay(400);

        return await context.SystemLogs
            .AsNoTracking()
            .OrderByDescending(l => l.Timestamp)
            .Take(20)
            .ToListAsync();
    }

    private async Task<UserStats?> GetUserStatsAsync(int userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        
        // Simulating slow query
        // await Task.Delay(300);

        return await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserStats(u.Orders.Count, u.Username))
            .FirstOrDefaultAsync();
    }
}
