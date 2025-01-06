using InterDemoStack.Configuration;
using InterviewDemoStack.Services;
using InterviewDemoStackFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlackLogger;
using System;
using System.Reflection;
using InterviewDemoStackFunctions.Extensions;

var hostBuilder = new HostBuilder();

hostBuilder.ConfigureAppConfiguration((context, configBuilder) =>
{
    var hostingEnvironment = Environment.GetEnvironmentVariable("HostingEnvironment");

    if (string.Equals(hostingEnvironment, "Dev", StringComparison.OrdinalIgnoreCase))
    {
        configBuilder
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .AddEnvironmentVariables();
    }
    else
    {
        configBuilder
            .SetBasePath(AppContext.BaseDirectory)
            .AddEnvironmentVariables()
            .AddAzureAppConfiguration(options =>
            {
                var configConnectionString = Environment.GetEnvironmentVariable("ConfigConnectionString");
                if (string.IsNullOrEmpty(configConnectionString))
                {
                    throw new InvalidOperationException("ConfigConnectionString is not set in the environment variables.");
                }

                options.Connect(configConnectionString)
                       .Select("AzureConfig:*");
            });
    }
});

hostBuilder.ConfigureFunctionsWorkerDefaults();

hostBuilder.ConfigureServices((context, services) =>
{
    var configuration = context.Configuration;

    services.AddAppServices(configuration);
});

var host = hostBuilder.Build();

host.Run();
