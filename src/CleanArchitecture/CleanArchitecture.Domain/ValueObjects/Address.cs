using CleanArchitecture.Domain.Common;

namespace CleanArchitecture.Domain.ValueObjects;

public class Address : ValueObject
{
    public String Street { get; private set; }
    public String City { get; private set; }
    public String Country { get; private set; }
    public String ZipCode { get; private set; }

    public Address() { } // EF Core

    public Address(string street, string city, string country, string zipCode)
    {
        Street = street;
        City = city;
        Country = country;
        ZipCode = zipCode;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return Country;
        yield return ZipCode;
    }
}
