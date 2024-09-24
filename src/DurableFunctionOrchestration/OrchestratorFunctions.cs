using DurableFunctionOrchestration.Activities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace FunctionApp1
{
    public static class OrchestratorFunctions
    {
        [Function(nameof(OrchestratorFunctions))]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(OrchestratorFunctions));
            logger.LogInformation("Starting the orchestrator.");

            var outputs = new List<string>();

            string? userId = context.GetInput<string>();

            logger.LogInformation(userId);

            outputs.Add(await context.CallActivityAsync<string>(nameof(HotelFunctions.RegistrationAsync), userId));

            // TODO: Create Flight 
            // TODO: Trip Confirmation



            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

       

        
    }
}
