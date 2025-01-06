using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewDemoStackFunctions.Services.Interfaces
{
    public interface IShouldQueueEventGridMessages
    {
        Task<bool> QueueEventGridMessages();
    }
}
