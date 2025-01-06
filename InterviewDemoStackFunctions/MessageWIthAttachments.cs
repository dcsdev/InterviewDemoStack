using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace InterviewDemoStackFunctions
{
    public class MessageWithAttachments
    {
        public Message MovedMessage { get; set; }
        public IList<Attachment> Attachments { get; set; }
    }

}
