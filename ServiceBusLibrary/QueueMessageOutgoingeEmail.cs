using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusLibrary.QueueMessaging
{
    public class QueueMessageOutgoingeEmail
    {
        public string ToAddress { get; set; } = "douglasspencer@douglasdevelops.onmicrosoft.com";
        public string FromAddress { get; set; } = "douglasspencer@douglasdevelops.onmicrosoft.com";
        public string Subject { get; set; } = "Email From Azure Demo Stack";
        public string Body { get; set; } = "Replace With Actual Body";
        public string HTMLBody { get; set; } = "<h1>Replace With Actual Body</h1>";
    }
}
