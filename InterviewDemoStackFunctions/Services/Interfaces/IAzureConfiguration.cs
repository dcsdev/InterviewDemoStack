using InterDemoStack.Configuration;

namespace InterviewDemoStackFunctions.Services.Interfaces
{
    public interface IAzureConfiguration
    {
        AzureConfig BuildAzureConfig();

        AzureConfig AzureConfig { get; }
    }
}
