using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewDemoStackFunctions.Services.Interfaces
{
    public interface IEventGridClient
    {
        Task PublishEventAsync<T>(string eventType, string subject, T data, string dataVersion = "1.0");
    }
}
