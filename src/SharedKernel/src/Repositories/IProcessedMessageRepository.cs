using SharedKernel.Entity;

namespace SharedKernel.Repositories;

public interface IProcessedMessageRepository
{
    Task InsertAsync(ProcessedMessage message);
    Task<bool> CheckExist(Guid messageId);
}