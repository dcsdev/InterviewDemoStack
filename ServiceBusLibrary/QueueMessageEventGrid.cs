using ServiceBusLibrary.QueueMessaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusLibrary.QueueMessaging
{
    public class QueueMessageEventGrid : QueueMessageBase
    {
        public EventGridMessage EventGridModel { get; set; } = new EventGridMessage();
    }
}
