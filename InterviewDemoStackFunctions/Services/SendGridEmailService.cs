using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterDemoStack.Configuration;
using InterviewDemoStack.Services;
using InterviewDemoStackFunctions.Extensions;
using InterviewDemoStackFunctions.Services.Interfaces;
using Microsoft.Graph.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using ServiceBusLibrary.QueueMessaging;
using EmailAddress = SendGrid.Helpers.Mail.EmailAddress;

namespace InterviewDemoStackFunctions.Services
{


    public class SendGridEmailService : IEmailService, ICanSendServiceBusMessage
    {
        private readonly string _sendGridApiKey;
        private readonly IAzureConfiguration _azureConfig;
        private readonly IQueueService _queueService;

        public SendGridEmailService(IAzureConfiguration azureConfiguration, IQueueService queueService)
        {
            _azureConfig = azureConfiguration;
            _sendGridApiKey = azureConfiguration.AzureConfig.OutboundEmail.SendGridAPIKey;
            _queueService = queueService;   
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var client = new SendGridClient(_azureConfig.AzureConfig.OutboundEmail.SendGridAPIKey);
            var email = new SendGridMessage
            {
                From = new EmailAddress(_azureConfig.AzureConfig.OutboundEmail.FromEmailName, _azureConfig.AzureConfig.OutboundEmail.FromEmailName),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };

            email.AddTo(new EmailAddress(toEmail));

            await client.SendEmailAsync(email);

            string eventTypeDescription = EventGridEventType.AzureDemoStack_OutboundEmailSent.GetDescription();

            EventGridMessage eventGridMessage = new EventGridMessage();
            eventGridMessage.Subject = eventTypeDescription;
            eventGridMessage.DataVersion = "1.0";
            eventGridMessage.Data.Message = eventTypeDescription;

            await SendMessageToQueueAsync(eventGridMessage, _azureConfig.AzureConfig.ServiceBus.EventGridQueueName);

        }

        public Task<bool> SendMessagesToQueueAsync<T>(List<T> messages, string queueName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SendMessageToQueueAsync<T>(T message, string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new InvalidOperationException("Queue name cannot be null or empty.");
            }

            await _queueService.SendMessageAsync(message, queueName);

            return true;
        }
    }

}
