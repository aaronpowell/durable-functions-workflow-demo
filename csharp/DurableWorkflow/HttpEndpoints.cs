using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DurableWorkflow
{
    public static class HttpEndpoints
    {
        [FunctionName("StartWorkflow")]
        public static async Task<IActionResult> RunStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "start/{input}")] HttpRequest req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            string input,
            ILogger log)
        {
            log.LogInformation($"Starting a new workflow for {input}");

            await starter.StartNewAsync(nameof(Workflow), input);

            return new OkResult();
        }

        [FunctionName("CheckWorkflow")]
        public static async Task<IActionResult> RunCheck(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "check/{input}")] HttpRequest req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            string input,
            ILogger log)
        {
            log.LogInformation($"Checking workflow for {input}");

            var offset = TimeSpan.FromMinutes(20);
            var time = DateTime.UtcNow;

            var instances = await starter.GetStatusAsync(
                time.Subtract(offset),
                time.Add(offset),
                new List<OrchestrationRuntimeStatus>()
            );

            return new OkObjectResult(new
            {
                instances,
                matchingInstances = instances.Where(i => i.Name == nameof(Workflow) && i.Input.ToObject<string>() == input)
            });
        }

        [FunctionName("StopWorkflow")]
        public static async Task<IActionResult> RunStop(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stop/{input}")] HttpRequest req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            string input,
            ILogger log)
        {
            log.LogInformation($"Stopping workflow for {input}");

            var offset = TimeSpan.FromMinutes(20);
            var time = DateTime.UtcNow;

            var instances = await starter.GetStatusAsync(
                time.Subtract(offset),
                time.Add(offset),
                new List<OrchestrationRuntimeStatus>()
            );

            var instance = instances.FirstOrDefault(i => i.Name == nameof(Workflow) && i.Input.ToObject<string>() == input);

            if (instance != null)
            {
                log.LogInformation($"Found a matching instance with id ${instance.InstanceId}");
                await starter.RaiseEventAsync(instance.InstanceId, nameof(Workflow));

                return new OkObjectResult(new
                {
                    instances = new[] { instance }
                });
            }
            else
            {
                log.LogInformation($"Didn't find a matching instance for {input}");
                return new NotFoundObjectResult(new
                {
                    instances
                });
            }
        }
    }
}
