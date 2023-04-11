using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Domain.Events;
using MediatR;
using Newtonsoft.Json;

namespace Application.Documents.EventHandlers;

public class TextPublishedEventHandler : INotificationHandler<TextPublishedEvent>
{
    private const string EventHubConnectionString =
        "Endpoint=sb://elasticsearch001.servicebus.windows.net/;SharedAccessKeyName=es;SharedAccessKey=tT1Jc2EumZuy9eeNQwYVLhFWHIb4rWawH+AEhPrS7ZY=;EntityPath=elasticsearch";
    private const string EventHubName = "elasticsearch";
    
    private readonly EventHubProducerClient _producerClient;
    
    public TextPublishedEventHandler()
    {
        _producerClient = new EventHubProducerClient(EventHubConnectionString, EventHubName);
    }

    public async Task Handle(TextPublishedEvent textPublishedEvent, CancellationToken cancellationToken)
    {
        using EventDataBatch eventBatch = await _producerClient.CreateBatchAsync();
        var eventData = new EventData(JsonConvert.SerializeObject(textPublishedEvent.Document));
        eventBatch.TryAdd(eventData);
        
        await _producerClient.SendAsync(eventBatch);
    }
}