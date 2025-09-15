using SharedKernel.ValueObjects;

namespace Contract.SharedDTOs;

public class GetMoneyDTO
{
    public long Amount { get; set; }
    public string Currency { get; set; }
}

public class CreateMoneyDTO
{
    public long Amount { get; set; }
    public CurrencyCode Currency { get; set; }
}