using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Domain;
using Microsoft.Graph.Models;
using ServiceBusLibrary.QueueMessaging;
using InterviewDemoStackFunctions.Extensions;
using InterviewDemoStackFunctions.Services.Interfaces;

namespace InterviewDemoStackFunctions
{
    public class FuncEmailMonitor : AzureFunctionBase, IShouldQueueEventGridMessages
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly DemoStackContext _demoStackContext;

        public FuncEmailMonitor(
               ILogger<FuncEmailMonitor> logger,
               IQueueService queueService,
               IAzureConfiguration azureConfig,
               IGraphServiceClientFactory graphFactory,
               DemoStackContext demoStackContext
           ) : base(logger, queueService, azureConfig)
        {
            _graphServiceClient = graphFactory.GetAuthenticatedGraphServiceClient();
            _demoStackContext = demoStackContext;
        }

        [Function("EmailMonitor")]
        public async Task RunAsync([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"Execution Start Time: {DateTime.Now}");

            try
            {
                ValidateConfiguration();

                var messagesFromGraph = await FetchEmailsAsync();

                if (!messagesFromGraph.Any())
                {
                    LogInformation("No messages found to process.");
                    return;
                }

                foreach (var message in messagesFromGraph)
                {
                    await ProcessEmailAsync(message);
                }
            }
            catch (Exception ex)
            {
                EventGridMessage eventGridMessage = new EventGridMessage();
                eventGridMessage.Subject = ex.Message;
                eventGridMessage.DataVersion = "1.0";
                eventGridMessage.Data.Message = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_ErrorProcessingEmails.GetDescription();

                await _queueService.SendMessageAsync(eventGridMessage, _azureConfig.AzureConfig.ServiceBus.EventGridQueueName);

                LogError($"Error processing emails: {ex.Message}", ex);
            }

            await QueueEventGridMessages();

            LogInformation($"Execution End Time: {DateTime.Now}");
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(_azureConfig.AzureConfig.AzureIdentity.UserId))
                throw new ArgumentNullException(nameof(_azureConfig.AzureConfig.AzureIdentity.UserId), "User ID cannot be null or empty.");

            if (Convert.ToInt32(_azureConfig.AzureConfig.EmailCapture.EmailsPerLoopToProcess) <= 0)
                throw new ArgumentException("EmailsPerLoopToProcess must be greater than zero.");
        }

        private async Task<IList<Message>> FetchEmailsAsync()
        {
            LogInformation("Fetching emails from Graph API.");

            var response = await _graphServiceClient
                .Users[_azureConfig.AzureConfig.AzureIdentity.UserId]
                .MailFolders["Inbox"]
                .Messages
                .GetAsync(requestConfig =>
                {
                    requestConfig.QueryParameters.Top = Convert.ToInt32(_azureConfig.AzureConfig.EmailCapture.EmailsPerLoopToProcess);
                    requestConfig.QueryParameters.Filter = "contains(subject, 'demo-stack')";
                    requestConfig.QueryParameters.Expand = new[] { "attachments" };
                });

            return response.Value;
        }

        private async Task ProcessEmailAsync(Microsoft.Graph.Models.Message message)
        {
            try
            {
                var orignalGraphId = message.Id;
                var finalGraphId = String.Empty;

                var newId = Guid.NewGuid().ToString();
                var GuidForNewId = Guid.Parse(newId);

                var domainMessage = new GraphEmailMessage
                {
                    Id = GuidForNewId,
                    Subject = message.Subject,
                    PartitionKey = newId,
                    Attachments = message.Attachments?.Select(a => new Domain.Attachment
                    {
                        Name = a.Name,
                        Size = a.Size,
                        IsInline = a.IsInline
                    }).ToList()
                };

                var movedMessage = await MoveMessageToCompletedFolderAsync(message);

                string cosmosEntityId = await SaveMessageToDatabaseAsync(domainMessage);

                if (movedMessage.MovedMessage.HasAttachments == true)
                {
                    foreach (var attachment in movedMessage.Attachments)
                    {
                        await AddAttachmentForProcessingAsync(movedMessage.MovedMessage.Id, attachment.Id, cosmosEntityId);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error processing email with ID {message.Id}: {ex.Message}", ex);
            }
        }

        private async Task AddAttachmentForProcessingAsync(string MailMessageGraphResourceLocatorId, string AttachmentGraphResourceLocatorId, string cosmosEntityId)
        {
            if (string.IsNullOrWhiteSpace(MailMessageGraphResourceLocatorId) || string.IsNullOrWhiteSpace(AttachmentGraphResourceLocatorId))
            {
                LogWarning("Invalid GraphId or AttachmentId provided.");
                return;
            }

            var queueMessage = new QueueMessageAttachmentProcessing
            {
                AttachmentGraphResourceLocatorId = AttachmentGraphResourceLocatorId,
                MailMessageGraphResourceLocatorId = MailMessageGraphResourceLocatorId,
                RelatedCosmosDbId = cosmosEntityId
            };

            try
            {
                if (string.IsNullOrWhiteSpace(_azureConfig.AzureConfig.ServiceBus.AttachmentsQueueName) || string.IsNullOrWhiteSpace(_azureConfig.AzureConfig.ServiceBus.ServiceBusConnectionString))
                {
                    throw new InvalidOperationException("QueueName or AzureServiceBus configuration is missing.");
                }

                await _queueService.SendMessageAsync(queueMessage, _azureConfig.AzureConfig.ServiceBus.AttachmentsQueueName);
                
                string eventTypeDescription = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_AttachmentProcessingStarted.GetDescription();

                EventGridMessage eventGridMessage = new EventGridMessage();
                eventGridMessage.Subject = eventTypeDescription;
                eventGridMessage.DataVersion = "1.0";
                eventGridMessage.Data.Message = eventTypeDescription;

                eventGridMessages.Add(eventGridMessage);

                LogInformation($"Message added to queue: {Task.CurrentId}");
            }
            catch (Exception ex)
            {
                LogError($"Error adding attachment to queue: {ex.Message}", ex);
            }
        }

        private async Task<String> SaveMessageToDatabaseAsync(GraphEmailMessage message)
        {
            await _demoStackContext.Database.EnsureCreatedAsync();

            message.Activities.Add(new Activity() { Type = Domain.ActivityType.ProcessedEmail, ActivityTypeString = Domain.ActivityType.RequestAttachmentProcessor.GetDescription() });
            message.Activities.Add(new Activity() { Type = Domain.ActivityType.RequestAttachmentProcessor, ActivityTypeString = Domain.ActivityType.RequestAttachmentProcessor.GetDescription() });

            var messageEntry = await _demoStackContext.Messages.AddAsync(message);
            await _demoStackContext.SaveChangesAsync();

            string eventTypeDescription = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_AttachmentProcessingStarted.GetDescription();

            EventGridMessage eventGridMessage = new EventGridMessage();
            eventGridMessage.Subject = eventTypeDescription;
            eventGridMessage.DataVersion = "1.0";
            eventGridMessage.Data.Message = eventGridMessage.Subject = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_EmailProcessed.GetDescription();

            eventGridMessages.Add(eventGridMessage);

            return messageEntry.Entity.Id.ToString();
        }

        private async Task<MessageWithAttachments> MoveMessageToCompletedFolderAsync(Microsoft.Graph.Models.Message message)
        {
            try
            {
                var completedFolder = await _graphServiceClient
                    .Users[_azureConfig.AzureConfig.AzureIdentity.UserId]
                    .MailFolders
                    .GetAsync();

                var completedFolderId = completedFolder.Value.FirstOrDefault(f => f.DisplayName == "Completed")?.Id;

                if (completedFolderId == null)
                {
                    var newFolder = await _graphServiceClient
                        .Users[_azureConfig.AzureConfig.AzureIdentity.UserId]
                        .MailFolders
                        .PostAsync(new MailFolder { DisplayName = "Completed" });

                    completedFolderId = newFolder.Id;
                }

                var requestBody = new Microsoft.Graph.Users.Item.Messages.Item.Move.MovePostRequestBody
                {
                    DestinationId = completedFolderId

                };

                var movedMessage = await _graphServiceClient
                    .Users[_azureConfig.AzureConfig.AzureIdentity.UserId]
                    .Messages[message.Id]

                    .Move
                    .PostAsync(requestBody);

                var movedMessageWithAttachments = await _graphServiceClient
                    .Users[_azureConfig.AzureConfig.AzureIdentity.UserId]
                    .Messages[movedMessage.Id]
                    .Attachments
                    .GetAsync();

                var messageWithAttachments = new MessageWithAttachments
                {
                    MovedMessage = movedMessage,
                    Attachments = movedMessageWithAttachments.Value
                };

                LogInformation($"Message with ID {message.Id} moved to Completed folder.");

                EventGridMessage eventGridMessage = new EventGridMessage();
                eventGridMessage.Subject = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_EmailProcessed.GetDescription();
                eventGridMessage.DataVersion = "1.0";
                eventGridMessage.Data.Message = eventGridMessage.Subject = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_EmailProcessed.GetDescription();

                eventGridMessages.Add( eventGridMessage );

                return messageWithAttachments;
            }
            catch (Exception ex)
            {
                EventGridMessage eventGridMessage = new EventGridMessage();
                eventGridMessage.Subject = ex.Message;
                eventGridMessage.DataVersion = "1.0";
                eventGridMessage.Data.Message = eventGridMessage.Subject = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_ErrorProcessingEmails.GetDescription();

                eventGridMessages.Add( eventGridMessage );

                LogError($"Error moving message to Completed folder: {ex.Message}", ex);
                
                return null;
            }
        }

        public async Task<bool> QueueEventGridMessages()
        {
            await PublishEventGridMessageAsync();

            return true;
        }
    }
}
