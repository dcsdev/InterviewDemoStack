using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusLibrary.QueueMessaging
{
    public class QueueMessageBase
    {
        public string Id { get; set; } = String.Empty;
        public string RelatedCosmosDbId { get; set; } = String.Empty;
        public string EventGridMessageType { get; set; } = String.Empty;
        public EventData EventGridData { get; set; } = new EventData();
    }
}
