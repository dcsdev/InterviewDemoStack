using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewDemoStackFunctions.Services.Interfaces
{
    public interface ICanSendServiceBusMessage
    {
        Task<bool> SendMessageToQueueAsync<T>(T message, string queueName);
        Task<bool> SendMessagesToQueueAsync<T>(List<T> messages, string queueName);
    }
}
