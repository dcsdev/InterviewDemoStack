using Microsoft.Graph;

namespace InterviewDemoStackFunctions.Services.Interfaces
{
    public interface IGraphServiceClientFactory
    {
        public GraphServiceClient GetAuthenticatedGraphServiceClient();
    }
}
