This repository contains a simple [Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?WT.mc_id=durablefunctionsworkflowdemo-github-aapowell) workflow demo, written in C#, F# and JavaScript.

# Getting Started

1. Clone or download the coode and open the folder of the language you want to use
2. Open the folder in [VS Code](https://code.visualstudio.com/?WT.mc_id=durablefunctionsworkflowdemo-github-aapowell)
  * You'll need the [VS Code Azure Functions extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions&WT.mc_id=durablefunctionsworkflowdemo-github-aapowell) installed
3. Edit the `local.settings.json` file to set the `AzureWebJobsStorage` connection string properly
  * If you're on Windows you can use the [Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator?WT.mc_id=durablefunctionsworkflowdemo-github-aapowell)
  * If you're on Mac or Linux you'll either need an Azure account to make a storage account ([sign up for a free trial here](https://azure.microsoft.com/en-us/free/?wt.mc_id=durablefunctionsworkflowdemo-github-aapowell)) or [Azurite](https://github.com/azure/azurite) for a local emulator
4. Start the debugger

# How to run the demo

When the demo functions have started up they will consist of 3 API endpoints:

* `/api/start/{input}`
  * Starts a new workflow for the input that you provide, eg: `/api/start/aaron`
* `/api/check/{input}`
  * Checks the workflow status for the input you started, eg: `/api/check/aaron`
  * Returns a JSON object showing all workflows in the storage account and the ones that match the provided input
* `/api/stop/{input}`
  * Sends an event to the workflow manager to stop the workflow for the input provided, eg: `/api/stop/aaron`
  * Returns a JSON object with the instance that was stopped or 404 if the input didn't match a workflow

# How it works

The demo creates 4 Azure functions, 3 of which have a [`HttpTrigger` binding](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook) and 1 [`OrchestrationTrigger` binding](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-bindings#orchestration-triggers).

Each HTTP Function also receives the [`OrchestrationClient`](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-bindings#orchestration-client) input which allows them to interact with Durable Functions.

The `Workflow` Function will be triggered by the `StartWorkflow` function and then it will wait for an external event to be triggered. While it's waiting the function is in a 'sleep' state and not consuming any resources, but when the event is triggered it will start at the point it had run up to and continue on (in this case run to completion). The event is triggered in the `StopWorkflow` Function.

# License

Aaron Powell - 2019
