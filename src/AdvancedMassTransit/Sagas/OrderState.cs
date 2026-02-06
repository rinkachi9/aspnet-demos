using MassTransit;
using System.ComponentModel.DataAnnotations;

namespace AdvancedMassTransit.Sagas;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    
    // CURRENT STATE (String)
    [MaxLength(64)]
    public string CurrentState { get; set; }

    // Business Data
    public string CustomerNumber { get; set; }
    public decimal TotalAmount { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    
    // Request/Response Metadata
    public Guid? RequestId { get; set; }
    public Uri? ResponseAddress { get; set; }
    
    [Timestamp]
    public uint Version { get; set; } // Optimistic Concurrency
}
