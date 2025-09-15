namespace SharedKernel.Entity;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
}