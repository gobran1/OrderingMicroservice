namespace SharedKernel.ValueObjects;

public enum CurrencyCode
{
    USD,
    EUR
}

public class Money : ValueObject
{
    public long Amount { get; set; }
    public CurrencyCode Currency { get; set; }

    public Money()
    {
        Amount = 0;
        Currency = CurrencyCode.USD;
    }
    
    public Money(long amount)
    {
        Amount = amount;
        Currency = CurrencyCode.USD;
    }
    
    public Money(long amount, CurrencyCode currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}