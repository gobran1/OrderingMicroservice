using Contract.Inventory.Events;
using MassTransit;
using MediatR;
using Order.Application.Common.Interfaces;
using Order.Application.Features.Order.Commands;
using Platform.Observability;
using SharedKernel.Entity;

namespace Order.Infrastructure.Consumer;

public class SalesStockReservedConsumer : IConsumer<SalesStockReservedEventV1>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public SalesStockReservedConsumer(IUnitOfWork unitOfWork,IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }
    
    public async Task Consume(ConsumeContext<SalesStockReservedEventV1> context)
    {
        if (await _unitOfWork.ProcessedMessageRepository.CheckExist(context.Message.Id))
            return;
    
        await _unitOfWork.ProcessedMessageRepository.InsertAsync(new ProcessedMessage()
        {
            MessageId = context.Message.Id,
            ProcessedAt = DateTime.UtcNow
        });
        
        var order = context.Message.OrderId;
        
       await  _mediator.Send(new FulfillOrderCommand(context.Message.OrderId));

       await _unitOfWork.SaveChangesAsync();
       
       BusinessMetrics.OrdersFulfilled.Add(1,
           new KeyValuePair<string, object?>("order_id", context.Message.OrderId.ToString()));
       
    }
}