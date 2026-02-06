using AdvancedMassTransit.Contracts;
using MassTransit;

namespace AdvancedMassTransit.Sagas;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        // Define which property holds the state
        InstanceState(x => x.CurrentState);

        // Define Events mapping
        Event(() => SubmitOrder, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderValidated, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderPaid, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => CheckStatus, x => 
        {
             x.CorrelateById(m => m.Message.OrderId);
             x.OnMissingInstance(m => m.ExecuteAsync(async context => 
             {
                 if (context.RequestId.HasValue)
                 {
                     await context.RespondAsync(new OrderStatusResult(context.Message.OrderId, "NotFound", "Unknown"));
                 }
             }));
        });

        // FLOW
        Initially(
            When(SubmitOrder)
                .Then(context => 
                {
                    context.Saga.CustomerNumber = context.Message.CustomerNumber;
                    context.Saga.TotalAmount = context.Message.TotalAmount;
                    context.Saga.CreatedAt = DateTime.UtcNow;
                })
                .TransitionTo(Submitted)
                // Publish event for Validation Microservice (or consumer)
                .Publish(context => new OrderSubmitted(context.Saga.CorrelationId, DateTime.UtcNow))
        );

        During(Submitted,
            When(OrderValidated)
                .Then(context => 
                {
                    if (!context.Message.IsValid) 
                        throw new Exception("Order Invalid!"); // Simple error handling demo
                })
                .TransitionTo(Validated)
        );
        
        During(Validated,
            When(OrderPaid)
                .Then(context => context.Saga.PaidAt = DateTime.UtcNow)
                .TransitionTo(Paid)
                // Finalize
                .Publish(context => new OrderCompleted(context.Saga.CorrelationId))
                .Finalize()
        );

        // REQUEST / RESPONSE Handler
        DuringAny(
            When(CheckStatus)
                .Respond(context => new OrderStatusResult(
                    context.Saga.CorrelationId, 
                    "Found", 
                    context.Saga.CurrentState))
        );

        SetCompletedWhenFinalized();
    }

    // STATES
    public State Submitted { get; private set; }
    public State Validated { get; private set; }
    public State Paid { get; private set; }

    // EVENTS
    public Event<SubmitOrder> SubmitOrder { get; private set; }
    public Event<OrderValidated> OrderValidated { get; private set; }
    public Event<OrderPaid> OrderPaid { get; private set; }
    public Event<CheckOrderStatus> CheckStatus { get; private set; }
}
