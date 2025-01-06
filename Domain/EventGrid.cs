using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class EventGrid
    {
        public string Id { get; set; }
        public string EventType { get; set; }
        public string Subject { get; set; }
        public EventData Data { get; set; }
        public DateTime EventTime { get; set; }
        public string DataVersion { get; set; }
    }

    public class EventData
    {
        public string Message
        {
            get; set;
        }
    }

    public enum EventGridEventType
    {
        [Description("An error was encountered processing emails")]
        AzureDemoStack_ErrorProcessingEmails,
        [Description("An email was received by the system.")]
        AzureDemoStack_EmailReceived,

        [Description("The email has been processed successfully.")]
        AzureDemoStack_EmailProcessed,

        [Description("An attachment has been sent for processing.")]
        AzureDemoStack_AttachmentProcessingStarted,

        [Description("The attachment has been processed successfully.")]
        AzureDemoStack_AttachmentProcessed,

        [Description("An attachment has been sent for PDF conversion.")]
        AzureDemoStack_PDFConversionStarted,

        [Description("The attachment has been successfully converted to a PDF.")]
        AzureDemoStack_PDFConversionCompleted,

        [Description("An outbound email has been queued for sending.")]
        AzureDemoStack_OutboundEmailQueued,

        [Description("An outbound email has been sent successfully.")]
        AzureDemoStack_OutboundEmailSent,

        [Description("A new blob was created in Azure Storage.")]
        Microsoft_Storage_BlobCreated,

        [Description("A blob was deleted from Azure Storage.")]
        Microsoft_Storage_BlobDeleted,

        [Description("A blob was renamed in Azure Storage.")]
        Microsoft_Storage_BlobRenamed,

        [Description("A resource write operation completed successfully.")]
        Microsoft_Resources_ResourceWriteSuccess,

        [Description("A resource write operation failed.")]
        Microsoft_Resources_ResourceWriteFailure,

        [Description("A resource write operation was canceled.")]
        Microsoft_Resources_ResourceWriteCancel,

        [Description("A capture file was created by Azure Event Hub.")]
        Microsoft_EventHub_CaptureFileCreated,

        [Description("Active messages are available but no listeners are present.")]
        Microsoft_ServiceBus_ActiveMessagesAvailableWithNoListeners,

        [Description("Deadletter messages are available but no listeners are present.")]
        Microsoft_ServiceBus_DeadletterMessagesAvailableWithNoListeners,

        [Description("A device connected to the Azure IoT Hub.")]
        Microsoft_IoTHub_DeviceConnected,

        [Description("A device disconnected from the Azure IoT Hub.")]
        Microsoft_IoTHub_DeviceDisconnected,

        [Description("A device registration operation completed in the provisioning service.")]
        Microsoft_DeviceProvisioningService_RegistrationOperationCompleted,

        [Description("A secret in Azure Key Vault is near expiry.")]
        Microsoft_KeyVault_SecretNearExpiry,

        [Description("A certificate in Azure Key Vault is near expiry.")]
        Microsoft_KeyVault_CertificateNearExpiry,

        [Description("A key in Azure Key Vault is near expiry.")]
        Microsoft_KeyVault_KeyNearExpiry,

        [Description("A key-value pair in Azure App Configuration was modified.")]
        Microsoft_AppConfiguration_KeyValueModified,

        [Description("A key-value pair in Azure App Configuration was deleted.")]
        Microsoft_AppConfiguration_KeyValueDeleted
    }

}