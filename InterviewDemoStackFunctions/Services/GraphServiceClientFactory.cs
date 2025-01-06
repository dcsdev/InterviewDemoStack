using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Extensions.Logging;
using InterDemoStack.Configuration;
using InterviewDemoStackFunctions.Services.Interfaces;

namespace InterviewDemoStack.Services
{
    public class GraphServiceClientFactory : IGraphServiceClientFactory
    {
        private readonly ILogger<GraphServiceClientFactory> _logger;
        private readonly IAzureConfiguration _azureConfig;

        public AzureConfig AzureConfig
        {
            get
            {
                return _azureConfig.AzureConfig;
            }
        }

        public GraphServiceClientFactory(ILogger<GraphServiceClientFactory> logger, IAzureConfiguration azureConfig)
        {
            _logger = logger;
            _azureConfig = azureConfig;
        }

        public GraphServiceClient GetAuthenticatedGraphServiceClient()
        {
            try
            {
                var tenantId = _azureConfig.AzureConfig.AzureIdentity.TenantId;
                var clientId = _azureConfig.AzureConfig.AzureIdentity.AppId;
                var clientSecret = _azureConfig.AzureConfig.AzureIdentity.ClientSecret;

                var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                var graphServiceClient = new GraphServiceClient(clientSecretCredential, new[] { "https://graph.microsoft.com/.default" });


                _logger.LogInformation("Initializing GraphServiceClient with Azure Identity.");

                return graphServiceClient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize GraphServiceClient.");
                throw;
            }
        }
    }
}
