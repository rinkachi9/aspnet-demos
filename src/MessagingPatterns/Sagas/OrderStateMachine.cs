using MassTransit;
using MessagingPatterns.Contracts;

namespace MessagingPatterns.Sagas;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => SubmitOrder, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderAccepted, x => x.CorrelateById(m => m.Message.OrderId));

        Initially(
            When(SubmitOrder)
                .Then(context => 
                {
                    context.Saga.CreatedAt = DateTime.UtcNow;
                })
                .TransitionTo(Submitted)
        );

        During(Submitted,
            When(OrderAccepted)
                .Then(context => 
                {
                    context.Saga.AcceptedAt = context.Message.Timestamp;
                })
                .TransitionTo(Accepted)
        );

        // Define States
    }

    public State Submitted { get; private set; } = null!;
    public State Accepted { get; private set; } = null!;

    public Event<SubmitOrder> SubmitOrder { get; private set; } = null!;
    public Event<OrderAccepted> OrderAccepted { get; private set; } = null!;
}
