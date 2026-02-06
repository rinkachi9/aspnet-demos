using FluentValidation;

namespace MinimalApiPipeline.Filters;

public class ValidationFilter<TRequest> : IEndpointFilter where TRequest : class
{
    private readonly IValidator<TRequest> _validator;

    public ValidationFilter(IValidator<TRequest> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Find the argument of type TRequest in the endpoint's arguments
        var argument = context.Arguments.SingleOrDefault(x => x?.GetType() == typeof(TRequest)) as TRequest;

        if (argument is null)
        {
            return Results.BadRequest(new { Error = "Invalid request body." });
        }

        var validationResult = await _validator.ValidateAsync(argument);

        if (!validationResult.IsValid)
        {
            // Return 400 Bad Request with validation errors (ProblemDetails structure ideally, simplified here as Dictionary)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }
}
