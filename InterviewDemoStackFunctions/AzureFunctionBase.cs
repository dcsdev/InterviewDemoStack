using InterviewDemoStackFunctions.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using ServiceBusLibrary.QueueMessaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewDemoStackFunctions
{
    public class AzureFunctionBase : ICanSendServiceBusMessage
    {
        protected readonly ILogger _logger;
        protected readonly IQueueService _queueService;
        protected readonly IAzureConfiguration _azureConfig;

        public List<EventGridMessage> eventGridMessages = new List<EventGridMessage>();

        protected AzureFunctionBase(ILogger logger, IQueueService queueService, IAzureConfiguration azureConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
            _azureConfig = azureConfig ?? throw new ArgumentNullException(nameof(azureConfig));
        }

        protected void LogInformation(string message)
        {
            _logger.LogInformation(message);
        }

        protected void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        protected void LogError(string message, Exception ex)
        {
            _logger.LogError($"{message}: {ex.Message}", ex);
        }

        protected async Task<bool> SendMessageToQueueAsync<T>(T message, string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new InvalidOperationException("Queue name cannot be null or empty.");
            }

            await _queueService.SendMessageAsync(message, queueName);

            return true;
        }

        async Task<bool> ICanSendServiceBusMessage.SendMessageToQueueAsync<T>(T message, string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new InvalidOperationException("Queue name cannot be null or empty.");
            }

            await _queueService.SendMessageAsync(message, queueName);

            return true;
        }

        public async Task<bool> SendMessagesToQueueAsync<T>(List<T> messages, string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new InvalidOperationException("Queue name cannot be null or empty.");
            }

            await _queueService.SendMessagesAsync(eventGridMessages, queueName);

            return true;
        }

        public async Task<bool> PublishEventGridMessageAsync()
        {
            try
            {
                if (eventGridMessages.Count > 0)
                {
                    await SendMessagesToQueueAsync(eventGridMessages, _azureConfig.AzureConfig.ServiceBus.EventGridQueueName);
                }
            }
            catch(Exception ex)
            {
                LogError(ex.Message, ex);
                return false;
            }

            return true;
        }
    }
}
