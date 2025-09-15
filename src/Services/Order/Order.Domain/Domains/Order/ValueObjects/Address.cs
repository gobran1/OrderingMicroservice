using SharedKernel.ValueObjects;

namespace Order.Domain.Domains.Order.ValueObjects;

public class Address : ValueObject
{
    public string? Country { get; private set; }
    public string? State { get; private set; }
    public string? City { get; private set; }
    public string? ZipCode { get; private set; }
    public string? Address1 { get; private set; }
    public string? Address2 { get; private set; }

    public Address()
    {
    }
    
    public Address(string? country, string? state, string? city, string? zipCode, string? address1, string? address2 = null)
    {
        Country = country;
        State = state;
        City = city;
        ZipCode = zipCode;
        Address1 = address1;
        Address2 = address2;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Country ?? string.Empty;
        yield return State ?? string.Empty;
        yield return City ?? string.Empty;
        yield return ZipCode ?? string.Empty;
        yield return Address1 ?? string.Empty;
        yield return Address2 ?? string.Empty;
    }
}