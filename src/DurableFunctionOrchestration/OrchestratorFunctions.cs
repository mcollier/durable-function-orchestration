using DurableFunctionOrchestration.Activities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
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

            TaskOptions retryOptions = TaskOptions.FromRetryHandler(retryContext =>
            {
                // Don't retry anything that derives from ApplicationException
                if(retryContext.LastFailure.IsCausedBy<HotelFunctionException>())
                {
                    return false;
                }

                // Quit after N attempts
                return retryContext.LastAttemptNumber < 3;
            });

            try
            {
                outputs.Add(await context.CallActivityAsync<string>(nameof(HotelFunctions.RegistrationAsync), userId, retryOptions));
            }
            catch(TaskFailedException e)
            {
                logger.LogError("Error Occured", e);
                Console.Out.WriteLine("error");
            }
            // TODO: Create Flight 
            // TODO: Trip Confirmation



            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

       

        
    }
}
