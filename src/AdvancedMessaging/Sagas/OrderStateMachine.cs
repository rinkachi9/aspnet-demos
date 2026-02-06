using AdvancedMessaging.Contracts;
using MassTransit;

namespace AdvancedMessaging.Sagas;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => SubmitOrder, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderValidated, x => x.CorrelateById(m => m.Message.OrderId));

        Initially(
            When(SubmitOrder)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.CustomerNumber = context.Message.CustomerNumber;
                    context.Saga.CreatedAt = DateTime.UtcNow;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .TransitionTo(Submitted)
                .Publish(context => new OrderAccepted(context.Saga.OrderId, DateTime.UtcNow))
        );

        During(Submitted,
            When(OrderValidated)
                .Then(context => context.Saga.UpdatedAt = DateTime.UtcNow)
                .TransitionTo(Validated)
        );

        SetCompletedWhenFinalized();
    }

    public State Submitted { get; private set; }
    public State Validated { get; private set; }

    public Event<SubmitOrder> SubmitOrder { get; private set; }
    public Event<OrderValidated> OrderValidated { get; private set; }
}
