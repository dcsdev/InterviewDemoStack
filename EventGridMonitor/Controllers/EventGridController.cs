using EventGridMonitorAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace EventGridMonitor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventGridController : ControllerBase
    {
        private static List<object> _eventLog = new List<object>();
        private readonly IHubContext<EventHub> _hubContext;

        public EventGridController(IHubContext<EventHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost]
        [Route("api/events")]
        public IActionResult ReceiveEvent([FromBody] List<EventGridEvent> events)
        {

            foreach (var evt in events)
            {
                _hubContext.Clients.All.SendAsync("ReceiveEvent", evt);

                _eventLog.Add(new
                {
                    evt.Id,
                    evt.EventType,
                    evt.EventTime,
                    evt.Data
                });
            }

            if (events.Count > 1 && events[events.Count].EventType == "Microsoft.EventGrid.SubscriptionValidationEvent")
            {
                var validationEvent = events[events.Count].Data as dynamic;
                return Ok(new { validationResponse = validationEvent.validationCode });
            }

            return Ok();

                
        }


        [HttpGet]
        [Route("api/events")]
        public IActionResult GetEvents()
        {
            return Ok(_eventLog);
        }

        // New endpoint to verify the API is running
        [HttpGet]
        [Route("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "API is running", timestamp = DateTime.UtcNow });
        }

    }

    public class EventGridEvent
    {
        public string Id { get; set; }
        public string EventType { get; set; }
        public string EventTime { get; set; }
        public object Data { get; set; }
    }
}
