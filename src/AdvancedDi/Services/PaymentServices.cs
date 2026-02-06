namespace AdvancedDi.Services;

public interface IPaymentService
{
    string ProcessPayment(decimal amount);
}

public class StripePaymentService : IPaymentService
{
    public string ProcessPayment(decimal amount) => $"Charged {amount:C} via Stripe";
}

public class DummyPaymentService : IPaymentService
{
    public string ProcessPayment(decimal amount) => $"[Dummy] Pretended to charge {amount:C}";
}
