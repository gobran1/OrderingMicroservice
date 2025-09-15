namespace Order.Application.Features.Order.DTOs;

public class AddressDTO
{
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
}

public class GetAddressDTO : AddressDTO;
public class CreateAddressDTO : AddressDTO;