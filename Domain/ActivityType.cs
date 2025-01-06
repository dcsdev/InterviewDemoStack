using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public enum ActivityType
    {
        [Description("Absolutely Nothing...")]
        None,
        [Description("Found and Processed Email")]
        ProcessedEmail,
        [Description("Requested Attachment Processing")]
        RequestAttachmentProcessor,
        [Description("An Attachment Has Been Processed")]
        AttachmentProcessed,
        [Description("An Attachment Has Been Requested To Be Converted")]
        RequestAttachmentConversion,
        [Description("An Attachment Has Been Converted To PDF")]
        AttachmentConverted,
        [Description("An Outbound Email Has Been Requested To Be Sent")]
        RequestOutboundEmail,
        [Description("An Outbound Email Has Been Sent")]
        OutboundEmailSent
    }
}
