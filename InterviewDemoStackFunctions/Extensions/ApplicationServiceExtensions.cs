using InterDemoStack.Configuration;
using InterviewDemoStack.Services;
using InterviewDemoStackFunctions.Services;
using InterviewDemoStackFunctions.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlackLogger;

namespace InterviewDemoStackFunctions.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
        {

            var eventGridConfig = configuration.GetSection("AzureConfig:EventGrid");

            services.AddSingleton<IEventGridClient>(sp =>
    new EventGridClient(eventGridConfig["TopicEndpoint"], eventGridConfig["TopicKey"]));


            services.AddSingleton<IEmailService, SendGridEmailService>();

            services.AddDbContext<DemoStackContext>(options =>
            {
                var cosmosConfig = configuration.GetSection("AzureConfig:CosmosDb");
                options.UseCosmos(cosmosConfig["AccountEndpoint"],cosmosConfig["AccountKey"],cosmosConfig["DatabaseName"]);});

            services.AddTransient<IGraphServiceClientFactory, GraphServiceClientFactory>();
            services.AddSingleton<IAzureConfiguration, AzureConfiguration>();

            services.AddLogging(logging =>
            {
                logging.AddConfiguration(configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();

                var slackConfig = configuration.GetSection("Logging:Slack").Get<SlackLoggingConfig>();
                if (slackConfig != null)
                {
                    logging.AddSlack(options =>
                    {
                        options.WebhookUrl = slackConfig.WebhookUrl;
                        options.LogLevel = LogLevel.Error;
                        options.NotificationLevel = LogLevel.Error;
                        options.Channel = slackConfig.Channel;
                        options.ApplicationName = slackConfig.ApplicationName;
                        options.UserName = slackConfig.UserName;
                        options.EnvironmentName = slackConfig.EnvironmentName;
                    });
                }
            });

            services.AddSingleton<IBlobStorageService>(provider =>
    new BlobStorageService(configuration["AzureConfig:Storage:FileStorageConnectionString"]));



            services.AddTransient<IQueueService, QueueService>();

           return services;
        }
    }
}
