namespace SharedKernel.Messaging;

public abstract record IntegrationEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Version { get; set; } = "v1";
    
    public string Name => GetType().Name;
}