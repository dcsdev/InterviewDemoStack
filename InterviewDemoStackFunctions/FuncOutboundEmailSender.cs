using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System.Text.Json;
using InterviewDemoStackFunctions.Services.Interfaces;
using ServiceBusLibrary.QueueMessaging;
using InterviewDemoStackFunctions.Extensions;

namespace InterviewDemoStackFunctions
{
    public class FuncOutboundEmailSender: AzureFunctionBase, IShouldQueueEventGridMessages
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IEmailService _emailService;

        private const string SourceContainerName = "source-container";
        private const string PdfContainerName = "pdf-container";
        private const string StorageConnectionString = "YourAzureStorageConnectionString";

        public FuncOutboundEmailSender(ILogger<FuncAttachmentConverter> logger, 
            IQueueService queueService, 
            IAzureConfiguration azureConfiguration, 
            IBlobStorageService blobStorageService, 
            IGraphServiceClientFactory graphFactory,
            IEmailService emailService) : base (logger, queueService, azureConfiguration)
        {
            _graphServiceClient = graphFactory.GetAuthenticatedGraphServiceClient();
            _blobStorageService = blobStorageService;
            _emailService = emailService;
        }

        [Function(nameof(FuncOutboundEmailSender))]
        public async Task Run(
            [ServiceBusTrigger("%OutboundEmailServiceBusQueueName%", Connection = "AzureServiceBus")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            QueueMessageOutgoingeEmail qm = JsonSerializer.Deserialize<QueueMessageOutgoingeEmail>(message.Body);

            try
            {
               await _emailService.SendEmailAsync(qm.ToAddress, qm.Subject, qm.Body);
                
                _logger.LogInformation("File Proccessed Successfuly");

                string eventTypeDescription = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_OutboundEmailSent.GetDescription();

                EventGridMessage eventGridMessage = new EventGridMessage();
                eventGridMessage.Subject = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_PDFConversionCompleted.GetDescription();
                eventGridMessage.DataVersion = "1.0";
                eventGridMessage.Data.Message = eventGridMessage.Subject = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_OutboundEmailQueued.GetDescription();

                await  SendMessageToQueueAsync(eventGridMessage, _azureConfig.AzureConfig.ServiceBus.EventGridQueueName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing file: {ex.Message}");
                await messageActions.DeferMessageAsync(message);
            }
        }

        public async Task<bool> QueueEventGridMessages()
        {
            await PublishEventGridMessageAsync();

            return true;
        }
    }
}
