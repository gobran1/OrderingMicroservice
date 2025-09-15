using SharedKernel.ValueObjects;

namespace Order.Domain.Domains.Order.ValueObjects;

public class ProductVO : ValueObject
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductSku { get; set; }

    public ProductVO()
    {
        
    }
    
    public ProductVO(Guid productId, string productName, string productSku)
    {
        ProductId = productId;
        ProductName = productName;
        ProductSku = productSku;
    }
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ProductId;
        yield return ProductName;
        yield return ProductSku;
    }
}