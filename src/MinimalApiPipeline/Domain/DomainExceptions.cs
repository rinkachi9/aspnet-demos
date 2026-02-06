namespace MinimalApiPipeline.Domain;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

public class UserAlreadyExistsException : DomainException
{
    public UserAlreadyExistsException(string email) 
        : base($"User with email '{email}' already exists.") { }
}
