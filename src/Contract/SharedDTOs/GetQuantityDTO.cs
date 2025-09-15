using SharedKernel.ValueObjects;

namespace Contract.SharedDTOs;

public class GetQuantityDTO
{
    public long Amount { get; set; }
    public string Uom { get; set; }
}

public class CreateQuantityDTO
{
    public long Amount { get; set; }
    public UOM Uom { get; set; }
}