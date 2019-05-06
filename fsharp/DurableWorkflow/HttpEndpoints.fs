module HttpEndpoints

open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Mvc
open System
open System.Threading

open Workflow

type CheckResponse =
     { Instances : seq<DurableOrchestrationStatus>
       MatchingInstances : seq<DurableOrchestrationStatus> }

type StopResponse = { Instances : seq<DurableOrchestrationStatus> }

[<FunctionName("StartWorkflow")>]
let startWorkflow
    ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "start/{input}")>] req : HttpRequest)
    ([<OrchestrationClient>] starter : DurableOrchestrationClient)
    input
    (logger : ILogger) =
    task {
        logger.LogInformation(sprintf "Starting a new workflow for %s" input)
        
        let! _ = starter.StartNewAsync(eventName, input)

        return OkResult()
    }

[<FunctionName("CheckWorkflow")>]
let checkWorkflow
    ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "check/{input}")>] req : HttpRequest)
    ([<OrchestrationClient>] starter : DurableOrchestrationClient)
    input
    (logger : ILogger) =
    task {
        logger.LogInformation(sprintf "Checking workflow for %s" input)

        let offset = TimeSpan.FromMinutes 20.
        let time = DateTime.UtcNow

        let! instances = starter.GetStatusAsync
                                 (time.Subtract offset,
                                  Nullable(time.Add offset),
                                  System.Collections.Generic.List<OrchestrationRuntimeStatus>(),
                                  CancellationToken.None)

        return OkObjectResult
            ({ Instances = instances
               MatchingInstances = instances
                                   |> Seq.filter (fun i -> i.Name = eventName && i.Input.ToObject<string>() = input) })
    }

[<FunctionName("StopWorkflow")>]
let stopWorkflow
    ([<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stop/{input}")>] req : HttpRequest)
    ([<OrchestrationClient>] starter : DurableOrchestrationClient)
    input
    (logger : ILogger) =
    task {
        logger.LogInformation(sprintf "Stopping workflow for %s" input)

        let offset = TimeSpan.FromMinutes 20.
        let time = DateTime.UtcNow

        let! instances = starter.GetStatusAsync
                                 (time.Subtract offset,
                                  Nullable(time.Add offset),
                                  System.Collections.Generic.List<OrchestrationRuntimeStatus>(),
                                  CancellationToken.None)

        return! match instances |> Seq.tryFind (fun i -> i.Name = eventName && i.Input.ToObject<string>() = input) with
                | Some instance ->
                    task {
                        logger.LogInformation(sprintf "Found a matching instance with id %s" instance.InstanceId)
                        do! starter.RaiseEventAsync(instance.InstanceId, eventName, input)
                        return OkObjectResult
                            ({ Instances = [|instance|] }) :> IActionResult
                    }

                | None ->
                    task {
                        sprintf "Didn't find a matching instance for %s" input |> logger.LogInformation

                        return NotFoundObjectResult
                            ({ Instances = instances }) :> IActionResult
                    }
    }