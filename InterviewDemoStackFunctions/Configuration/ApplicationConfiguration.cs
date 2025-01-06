using Microsoft.Extensions.Logging;

namespace InterDemoStack.Configuration
{
    public class AzureConfig
    {
        public AzureIdentity AzureIdentity { get; set; } = new AzureIdentity();
        public EmailCapture EmailCapture { get; set; } = new EmailCapture();
        public SharePoint SharePoint { get; set; } = new SharePoint();
        public ServiceBus ServiceBus { get; set; } = new ServiceBus();
        public Storage Storage { get; set; } = new Storage();
        public SlackLoggingConfig SlackLoggingConfig { get; set; } = new SlackLoggingConfig();

        public OutboundEmail OutboundEmail { get; set; } = new OutboundEmail();

        public EventGrid EventGrid { get; set; } = new EventGrid();
    }

    public class AzureIdentity
    {
        public string TenantId { get; set; }
        public string ClientSecret { get; set; }
        public string AppId { get; set; }
        public string UserId { get; set; }
    }

    public class EmailCapture
    {
        public string TestProp { get; set; }
        public string MatterNumberRegEx { get; set; }
        public string EmailsPerLoopToProcess { get; set; }
        public string FromEmailAddress { get; set; }
        public string ExceptionReportFromAddress { get; set; }
        public string ExceptionReportRecipients { get; set; }
        public string ExceptionReportCCRecipients { get; set; }
        public string ExceptionReportSubject { get; set; }

        public string SendGridApiKey { get; set; }
        public bool SkipEmbeddedImages { get; set; }

    }


    public class SharePoint
    {
        public bool SkipDuplicateUpload { get; set; }
        public int FilePathCharacterMax { get; set; }
        public string ExceptionsFolderID { get; set; }
        public string DuplicateFolderID { get; set; }
        public string ExceptionFolderName { get; set; }
        public string EmailFolderName { get; set; }
        public string DuplicateFolderName { get; set; }
        public string DriveId { get; set; }
        public string CompletedFolderId { get; set; }

        public long MaxReleaseRetryCount { get; set; }
        public long ReleaseAuditTimeSpanMinutes { get; set; }

    }

    public class SlackLoggingConfig
    {
        public LogLevel LogLevel { get; set; }
        public string ApplicationName { get; set; }
        public string EnvironmentName { get; set; }
        public LogLevel NotificationLevel { get; set; }
        public string UserName { get; set; }
        public string WebhookUrl { get; set; }
        public string Channel { get; set; }
    }

    public class Containers
    {
        public string Attachments = "attachments";
        public string PDF = "pdf";
    }

    public class Storage
    {
        public string FileStorageConnectionString { get; set; }
        public string AttachmentContainerName { get; } = "attachments";
        public string PDFContainerName { get; } = "pdfs";
    }

    public class ServiceBus
    {
        public string AttachmentsQueueName { get; } = "attachments";
        public string PDFConvertQueueName { get; } = "pdfconversion";
        public string EventGridQueueName { get; } = "event-grid-messages";
        public string ServiceBusConnectionString { get; set; }
    }

    public class OutboundEmail
    {
        public string SendGridAPIKey { get; set; }
        public string FromEmailAddress { get; set; }
        public string FromEmailName { get; set; }
        public string ToEmailAddress { get; set; }
    }

    public class EventGrid
    {
        public string TopicEndpoint { get; set; }
        public string TopicKey { get; set; }
    }
}
