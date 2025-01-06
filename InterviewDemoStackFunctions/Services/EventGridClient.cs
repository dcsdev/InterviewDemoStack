using InterviewDemoStackFunctions.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InterviewDemoStackFunctions.Services
{
    public class EventGridClient : IEventGridClient
    {
        private readonly string _topicEndpoint;
        private readonly string _topicKey;
        private readonly HttpClient _httpClient;

        public EventGridClient(string topicEndpoint, string topicKey)
        {
            _topicEndpoint = topicEndpoint ?? throw new ArgumentNullException(nameof(topicEndpoint));
            _topicKey = topicKey ?? throw new ArgumentNullException(nameof(topicKey));
            _httpClient = new HttpClient();
        }

        public async Task PublishEventAsync<T>(string eventType, string subject, T data, string dataVersion = "1.0")
        {
            var eventGridEvent = new[]
            {
            new
            {
                Id = Guid.NewGuid().ToString(),
                EventType = eventType,
                Subject = subject,
                EventTime = DateTime.UtcNow,
                Data = data,
                DataVersion = dataVersion
            }
        };

            string jsonContent = JsonConvert.SerializeObject(eventGridEvent);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _topicEndpoint)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

          //
            httpRequestMessage.Headers.Add("aeg-sas-key", _topicKey);

            HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage);
            if (!response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to publish event. Status Code: {response.StatusCode}, Details: {responseContent}");
            }
        }
    }
}
