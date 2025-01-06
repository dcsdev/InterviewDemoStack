using InterDemoStack.Configuration;
using InterviewDemoStackFunctions.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace InterviewDemoStack.Services
{
    public class AzureConfiguration : IAzureConfiguration
    {
        private readonly IConfiguration _configuration;
        private readonly AzureConfig _azureConfig = new AzureConfig();

        public AzureConfig AzureConfig { get { return _azureConfig; } }

        public AzureConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;

            BuildAzureConfig();
            
        }
        public AzureConfig BuildAzureConfig()
        {
            _azureConfig.AzureIdentity = _configuration.GetSection("AzureConfig:AzureIdentity").Get<AzureIdentity>() ?? new AzureIdentity();
            _azureConfig.EmailCapture = _configuration.GetSection("AzureConfig:EmailCapture").Get<EmailCapture>() ?? new EmailCapture();
            _azureConfig.ServiceBus = _configuration.GetSection("AzureConfig:ServiceBus").Get<ServiceBus>() ?? new ServiceBus();
            _azureConfig.Storage = _configuration.GetSection("AzureConfig:Storage").Get<Storage>() ?? new Storage();
            _azureConfig.OutboundEmail = _configuration.GetSection("AzureConfig:OutboundEmail").Get<OutboundEmail>() ?? new OutboundEmail();
            _azureConfig.EventGrid = _configuration.GetSection("AzureConfig:EventGrid").Get<EventGrid>() ?? new EventGrid();

            return _azureConfig;
        }
    }
}
