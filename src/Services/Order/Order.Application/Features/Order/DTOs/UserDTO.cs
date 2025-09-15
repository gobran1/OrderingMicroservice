namespace Order.Application.Features.Order.DTOs;

public class UserDTO
{
    public Guid? UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
}

public class GetUserDTO : UserDTO;
public class CreateUserVoDTO : UserDTO;