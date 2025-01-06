using System;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using InterviewDemoStackFunctions.Extensions;
using InterviewDemoStackFunctions.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ServiceBusLibrary.QueueMessaging;

namespace InterviewDemoStackFunctions
{
    public class FuncEventGridHandler
    {
        private readonly ILogger<FuncEventGridHandler> _logger;
        private readonly IEventGridClient _eventGridClient;
        private readonly DemoStackContext _demoStackContext;

        public FuncEventGridHandler(ILogger<FuncEventGridHandler> logger, IEventGridClient eventGridClient, DemoStackContext demoStackContext)
        {
            _logger = logger;
            _eventGridClient = eventGridClient;
            _demoStackContext = demoStackContext;
        }

        [Function(nameof(FuncEventGridHandler))]
        public async Task Run(
             [ServiceBusTrigger("%EventGridServiceBusQueueName%", Connection = "AzureServiceBus")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            EventGridMessage eventGridMessage = JsonSerializer.Deserialize<EventGridMessage>(message.Body);

            await _eventGridClient.PublishEventAsync(eventGridMessage.EventType.GetDescription(), eventGridMessage.Subject, new EventData() { Message = eventGridMessage.Data.Message, Tag = "Tag, You're It!" });

            await _demoStackContext.EventGridLogs.AddAsync(new Domain.EventGrid() { Subject = eventGridMessage.Subject, EventTime = DateTime.Now, EventType = eventGridMessage.EventType.GetDescription(), Data = new Domain.EventData() { Message = eventGridMessage.Data.Message}, DataVersion = eventGridMessage.DataVersion, Id = Guid.NewGuid().ToString()});
            await _demoStackContext.SaveChangesAsync();

            await messageActions.CompleteMessageAsync(message);
        }
    }
}
