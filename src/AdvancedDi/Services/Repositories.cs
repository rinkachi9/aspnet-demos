namespace AdvancedDi.Services;

public interface IRepository { }

public class UserRepository : IRepository
{
    public string GetUser() => "User A";
}

public class ProductRepository : IRepository
{
    public string GetProduct() => "Product B";
}
