using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using InterDemoStack.Configuration;
using Domain;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using InterviewDemoStackFunctions.Extensions;
using InterviewDemoStackFunctions.Services.Interfaces;
using InterviewDemoStackFunctions.Services;
using ServiceBusLibrary.QueueMessaging;

namespace InterviewDemoStackFunctions
{
    public class FuncAttachmentSaver : AzureFunctionBase, IShouldQueueEventGridMessages
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IBlobStorageService _blobStorageService;
        private readonly DemoStackContext _demoStackContext;

        public FuncAttachmentSaver(ILogger<FuncAttachmentSaver> logger, IQueueService queueService, IGraphServiceClientFactory graphFactory, IAzureConfiguration azureConfiguration, IBlobStorageService blobStorageService, DemoStackContext demoStackContext)
            : base(logger, queueService, azureConfiguration)
        {
            _graphServiceClient = graphFactory.GetAuthenticatedGraphServiceClient();
            _blobStorageService = blobStorageService;
            _demoStackContext = demoStackContext;   
        }

        public Task<bool> QueueEventGridMessages()
        {
            throw new NotImplementedException();
        }

        [Function(nameof(FuncAttachmentSaver))]
        public async Task Run(
            [ServiceBusTrigger("%AttachmentsServiceBusQueueName%", Connection = "AzureServiceBus")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            QueueMessageAttachmentProcessing qm = JsonSerializer.Deserialize<QueueMessageAttachmentProcessing>(message.Body);

            try
            {

                var attachment = await _graphServiceClient.Users[_azureConfig.AzureConfig.AzureIdentity.UserId]
    .Messages[qm.MailMessageGraphResourceLocatorId]
    .Attachments[qm.AttachmentGraphResourceLocatorId]
    .GetAsync();

                if (attachment is FileAttachment fileAttachment)
                {
                    var emailMessage = await _graphServiceClient.Users[_azureConfig.AzureConfig.AzureIdentity.UserId]
                        .Messages[qm.MailMessageGraphResourceLocatorId]
                        .GetAsync();

                    if (emailMessage != null)
                    {
                        var emailSubject = emailMessage.Subject;

                        var sanitizedSubject = string.Join("_", emailSubject.Split(Path.GetInvalidFileNameChars()));

                        var blobContainerName = _azureConfig.AzureConfig.Storage.AttachmentContainerName;
                        var blobName = $"{emailMessage.Id}/{sanitizedSubject}/{fileAttachment.Name}";

                        await _blobStorageService.UploadFileAsync(blobContainerName, blobName, fileAttachment.ContentBytes);

                        LogInformation($"Attachment '{fileAttachment.Name}' uploaded to folder '{sanitizedSubject}' in Blob Storage.");

                        await messageActions.CompleteMessageAsync(message);

                        if (!fileAttachment.Name.EndsWith(".pdf"))
                        {
                            QueueMessagePDFConversion qmPdfConversion = new QueueMessagePDFConversion();

                            qmPdfConversion.AttachmentGraphResourceLocatorId = fileAttachment.Id;
                            qmPdfConversion.FilePath = blobName;

                            await _queueService.SendMessageAsync(qmPdfConversion, _azureConfig.AzureConfig.ServiceBus.PDFConvertQueueName);

                            var result = await _demoStackContext.Messages.Where(m => m.PartitionKey == qm.RelatedCosmosDbId).FirstOrDefaultAsync();

                            result.Activities.Add(new Activity() { Id = new Guid(), Type = Domain.ActivityType.AttachmentProcessed, ActivityTypeString = Domain.ActivityType.AttachmentProcessed.GetDescription() });

                            await _demoStackContext.SaveChangesAsync();

                            string eventTypeDescription = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_PDFConversionStarted.GetDescription();

                            eventGridMessages.Add(new EventGridMessage()
                            {
                                DataVersion = "1.0",
                                Data = new ServiceBusLibrary.QueueMessaging.EventData() { Message = eventTypeDescription },
                                Subject = eventTypeDescription
                            });
                        }
                    }
                    else
                    {
                        LogInformation($"Unable to fetch email subject.");
                    }
                }
                else
                {
                    LogInformation($"Attachment is not a file.");

                    await messageActions.AbandonMessageAsync(message);
                }

                await PublishEventGridMessageAsync();
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex);
                throw;
            }
        }
    }
}
