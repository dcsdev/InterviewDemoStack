using ServiceBusLibrary.QueueMessaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusLibrary.QueueMessaging
{
    public class QueueMessageAttachmentProcessing : QueueMessageBase
    {
        public string AttachmentGraphResourceLocatorId { get; set; }
        public string MailMessageGraphResourceLocatorId { get; set; }
        
    }
}
