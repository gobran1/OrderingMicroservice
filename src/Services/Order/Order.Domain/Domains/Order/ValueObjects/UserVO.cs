using SharedKernel.ValueObjects;

namespace Order.Domain.Domains.Order.ValueObjects;

public class UserVO : ValueObject
{
    public Guid? UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }

    public UserVO()
    {
        
    }
    
    public UserVO(Guid? id, string name, string email)
    {
        UserId = id;
        UserName = name;
        UserEmail = email;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return UserId;
        yield return UserName;
        yield return UserEmail;
    }
}