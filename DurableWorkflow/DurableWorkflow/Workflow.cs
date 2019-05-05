using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DurableWorkflow
{
    public static class Workflow
    {
        [FunctionName("Workflow")]
        public static async Task Run(
            [OrchestrationTrigger]DurableOrchestrationContext context,
            ILogger logger
            )
        {
            var input = context.GetInput<string>();
            logger.LogInformation($"Starting workflow for {input}");

            await context.WaitForExternalEvent(nameof(Workflow));

            logger.LogInformation($"Workflow for {input} is stopping");
        }
    }
}
