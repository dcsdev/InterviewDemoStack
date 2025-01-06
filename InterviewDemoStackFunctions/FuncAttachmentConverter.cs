using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Domain;
using System.Text.Json;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Http;
using InterviewDemoStackFunctions.Services.Interfaces;
using ServiceBusLibrary.QueueMessaging;
using InterDemoStack.Configuration;
using InterviewDemoStackFunctions.Extensions;

namespace InterviewDemoStackFunctions
{
    public class FuncAttachmentConverter : AzureFunctionBase, IShouldQueueEventGridMessages
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IBlobStorageService _blobStorageService;

        private const string SourceContainerName = "source-container";
        private const string PdfContainerName = "pdf-container";
        private const string StorageConnectionString = "YourAzureStorageConnectionString";

        public FuncAttachmentConverter(
            ILogger<FuncAttachmentConverter> logger,
            IQueueService queueService,
            IGraphServiceClientFactory graphFactory,
            IAzureConfiguration azureConfiguration,
            IBlobStorageService blobStorageService
        ) : base(logger, queueService, azureConfiguration)
        {
            _graphServiceClient = graphFactory.GetAuthenticatedGraphServiceClient();
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        }

        [Function(nameof(FuncAttachmentConverter))]
        public async Task Run(
            [ServiceBusTrigger("%PDFConversionServiceBusQueueName%", Connection = "AzureServiceBus")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            QueueMessagePDFConversion qm = JsonSerializer.Deserialize<QueueMessagePDFConversion>(message.Body);
            var fileName = qm.FilePath;

            try
            {
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    var fileContents = await _blobStorageService.ReadFileAsync("attachments", fileName);

                    using (MemoryStream sourceStream = new MemoryStream())
                    {
                        sourceStream.Write(fileContents, 0, fileContents.Length);
                        sourceStream.Position = 0;

                        if (IsPdf(sourceStream))
                        {
                            LogInformation("File is already a PDF.");

                            await _blobStorageService.UploadFileAsync("pdfs", fileName, fileContents);
                            sourceStream.Position = 0;
                        }
                        else
                        {
                            LogInformation("Converting file to PDF...");
                            byte[] pdfBytes = ConvertToPdf(sourceStream);

                            await _blobStorageService.UploadFileAsync("pdfs", Path.ChangeExtension(fileName, "pdf"), pdfBytes);
                            await messageActions.CompleteMessageAsync(message);

                            eventGridMessages.Add(new EventGridMessage
                            {
                                Subject = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_PDFConversionCompleted.GetDescription(),
                                DataVersion = "1.0",
                                Data = new ServiceBusLibrary.QueueMessaging.EventData { Message = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_OutboundEmailQueued.GetDescription() }
                            });
                        }
                    }
                }

                _logger.LogInformation("File Processed Successfully");

                QueueMessageOutgoingeEmail qmOutboundEmail = new QueueMessageOutgoingeEmail();

                await _queueService.SendMessageAsync(qmOutboundEmail, Environment.GetEnvironmentVariable("OutboundEmailServiceBusQueueName"));

                EventGridMessage eventGridMessage = new EventGridMessage
                {
                    Subject = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_OutboundEmailQueued.GetDescription(),
                    DataVersion = "1.0",
                    Data = new ServiceBusLibrary.QueueMessaging.EventData { Tag = "My tag", Message = ServiceBusLibrary.QueueMessaging.EventGridEventType.AzureDemoStack_OutboundEmailQueued.GetDescription() }
                };

                eventGridMessages.Add(eventGridMessage);

                await PublishEventGridMessageAsync();
            }
            catch (Exception ex)
            {
                LogError($"Error processing file: {ex.Message}", ex);
                await messageActions.DeferMessageAsync(message);
            }
        }

        private static bool IsPdf(Stream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            stream.Position = 0;
            return BitConverter.ToString(buffer) == "25-50-44-46"; // PDF header "%PDF"
        }

        private byte[] ConvertToPdf(Stream sourceStream)
        {
            string fileText = "Sample Text";

            try
            {
                sourceStream.Seek(0, SeekOrigin.Begin);

                using (StreamReader reader = new StreamReader(sourceStream))
                {
                    fileText = reader.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(fileText))
                {
                    return new byte[0];
                }

                byte[] pdfBytes;
                using (var ms = new MemoryStream())
                {
                    using (var pdfWriter = new PdfWriter(ms))
                    {
                        using (var pdfDocument = new PdfDocument(pdfWriter))
                        {
                            var document = new Document(pdfDocument);
                            document.Add(new Paragraph(fileText));
                            document.Close();
                        }
                    }
                    pdfBytes = ms.ToArray();
                }

                return pdfBytes;
            }
            catch (Exception ex)
            {
                LogError($"An error occurred: {ex.Message}", ex);
                return new byte[0];
            }
        }

        public async Task<bool> QueueEventGridMessages()
        {
            await PublishEventGridMessageAsync();

            return true; ;
        }
    }
}
