using Azure.Messaging.ServiceBus;
using InterviewDemoStackFunctions.Services.Interfaces;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using ServiceBusLibrary.QueueMessaging;
using System.Text;
using System.Text.Json;

namespace InterviewDemoStackFunctions.Services
{
    
    public class QueueService : IQueueService
    {
        private readonly IConfiguration? _config;
        private readonly IEventGridClient _eventGridClient;


        public QueueService(IConfiguration config, IEventGridClient eventGridClient)
        {
            _config = config;
            _eventGridClient = eventGridClient;
        }

        public async Task SendMessageAsync<T>(T serviceBusMessage, string queueName)
        {
            ServiceBusClient sbClient = new ServiceBusClient(_config["AzureConfig:ServiceBus:ServiceBusConnectionString"]);

            ServiceBusSender sender = sbClient.CreateSender(queueName);

            string messageBody = JsonSerializer.Serialize(serviceBusMessage);
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody));
             
            await sender.SendMessageAsync(message);

        }

        public async Task SendMessagesAsync<T>(List<T> serviceBusMessages, string queueName)
        {
            serviceBusMessages.ForEach(async sbm =>
            {
                await SendMessageAsync(sbm, queueName);
            });
        }
    }
}
