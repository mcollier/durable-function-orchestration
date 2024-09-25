using DurableFunctionOrchestration.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionApp1
{
    internal class HttpFunctions
    {
        [Function(nameof(HttpStart))]
        public static async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(HttpStart));

            string userId = req.Query["userId"] ?? Guid.NewGuid().ToString("N");

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(OrchestratorFunctions.RunOrchestrator),
                userId);

            logger.LogInformation("Started orchestration with ID = '{instanceId}' for user {userId}.", instanceId, userId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            return await client.CreateCheckStatusResponseAsync(req, instanceId);
        }

        [Function(nameof(Approve))]
        public static async Task<IActionResult> Approve(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(Approve));

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<ApprovalRequest>(requestBody);
            if (data == null)
            {
                return new BadRequestObjectResult("Could not deserialize the request");
            }
            var instance = await client.GetInstanceAsync(data.InstanceId);
            if (instance == null)
            {
                return new NotFoundObjectResult("Instance not found");
            }
            if (instance.RuntimeStatus != OrchestrationRuntimeStatus.Running)
            {
                return new BadRequestObjectResult("Cannot change state because instance is not running");
            }
            await client.RaiseEventAsync(data.InstanceId, "Approval", data);
            return new AcceptedResult();
        }

        [Function(nameof(GetStatus))]
        public static async Task<IActionResult> GetStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            var instanceId = req.Query["instanceid"];
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return new BadRequestObjectResult("Instance not provided");
            }
            var instance = await client.GetInstanceAsync(instanceId, true);
            if (instance == null)
            {
                return new NotFoundObjectResult("Instance not found");
            }
            return new JsonResult(instance);
        }

    }
}
