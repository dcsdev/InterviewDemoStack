namespace InterviewDemoStackFunctions.Services.Interfaces
{
    public interface IQueueService
    {
        Task SendMessageAsync<T>(T serviceBusMessage, string queueName);
        Task SendMessagesAsync<T>(List<T> serviceBusMessage, string queueName);
    }
}