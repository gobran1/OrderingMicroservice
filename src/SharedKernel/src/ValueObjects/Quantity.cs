namespace SharedKernel.ValueObjects;


public enum UOM
{
    Piece,Kg,Liter
}

public class Quantity : ValueObject
{
    public long Amount { get; set; }
    public UOM Uom { get; set; }
    
    public Quantity()
    {
        Amount = 0;
        Uom = UOM.Piece;
    }
    
    public Quantity(long amount)
    {
        Amount = amount;
        Uom = UOM.Piece;
    }
    
    public Quantity(long amount, UOM uom)
    {
        Amount = amount;
        Uom = uom;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Uom;
    }
}