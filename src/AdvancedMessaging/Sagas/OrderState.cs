using MassTransit;

namespace AdvancedMessaging.Sagas;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;

    public Guid OrderId { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public int Version { get; set; } // Optimistic Concurrency
}
