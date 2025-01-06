using ServiceBusLibrary.QueueMessaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusLibrary.QueueMessaging
{
    public class QueueMessagePDFConversion : QueueMessageBase
    {
        public string AttachmentGraphResourceLocatorId { get; set; } = String.Empty;
        public string FilePath { get; set; } = String.Empty;
    }
}
