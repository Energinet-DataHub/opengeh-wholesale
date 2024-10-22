module "monitor_action_group_mig" {
  query_alerts_list = [
    {
      name        = "dropzoneunzipper-exception"
      description = "Triggers if there has been exceptions in the past hour in the dropzoneunzipper function"
      severity    = 1
      frequency   = 60
      time_window = 60
      operator    = "GreaterThan"
      threshold   = 0
      query       = <<QUERY
        exceptions
        | where cloud_RoleName in ("${module.func_dropzoneunzipper.name}")
        | summarize exceptionCount = count() by type
        | order by exceptionCount desc
        QUERY
    },
    {
      name        = "dropzoneunzipper-error-trace-severity"
      description = "Triggers if there has been any traces with error severity in the past hour in the dropzoneunzipper function"
      severity    = 1
      frequency   = 60
      time_window = 60
      operator    = "GreaterThan"
      threshold   = 0
      query       = <<QUERY
        traces
        | where cloud_RoleName in ("${module.func_dropzoneunzipper.name}")
        | where severityLevel == 3
        | where tostring(customDimensions["EventName"]) !in ("EventReceiveError", "EventProcessorPartitionProcessingError")
        | summarize eventCount = count() by tostring(customDimensions["EventName"])
        | order by eventCount desc
        QUERY
    }/*,
    {
      name        = "timeseriesretriever-exception"
      description = "Triggers if there has been exceptions in the past hour in the timeseriesretriever function"
      severity    = 1
      frequency   = 60
      time_window = 60
      operator    = "GreaterThan"
      threshold   = 0
      query       = <<QUERY
        exceptions
        | where cloud_RoleName in ("${module.func_timeseriesretriever.name}")
        | where type !in ("Microsoft.Azure.WebJobs.Script.Workers.Rpc.RpcException", "System.Threading.Tasks.TaskCanceledException")
        | where innermostMessage !contains "Non-Deterministic workflow detected: A previous execution of this orchestration scheduled an activity task with sequence ID 0"
        | summarize exceptionCount = count() by type
        | order by exceptionCount desc
        QUERY
    },
    {
      name        = "timeseriesretriever-error-trace-severity"
      description = "Triggers if there has been any traces with error severity in the past hour in the timeseriesretriever function"
      severity    = 1
      frequency   = 60
      time_window = 60
      operator    = "GreaterThan"
      threshold   = 0
      query       = <<QUERY
        traces
        | where cloud_RoleName in ("${module.func_timeseriesretriever.name}")
        | where severityLevel == 3
        | where tostring(customDimensions["EventName"]) !in ("OrchestrationProcessingFailure", "FunctionCompleted", "TaskActivityDispatcherError", "ProcessWorkItemFailed", "HealthCheckEnd")
        | where tostring(customDimensions["prop__reason"]) !contains ("DurableTask.Core.Exceptions.OrchestrationFailureException")
        | where tostring(customDimensions["prop__reason"]) !contains ("Microsoft.Azure.WebJobs.Host.FunctionInvocationException")
        | summarize eventCount = count() by tostring(customDimensions["EventName"])
        | order by eventCount desc
        QUERY
    },
    {
      name        = "timeseriesprocessor-exception"
      description = "Triggers if there has been exceptions in the past hour in the timeseriesprocessor function"
      severity    = 1
      frequency   = 60
      time_window = 60
      operator    = "GreaterThan"
      threshold   = 0
      query       = <<QUERY
        exceptions
        | where cloud_RoleName in ("${module.func_timeseriesprocessor.name}")
        | where type !in ("Microsoft.Azure.WebJobs.Script.Workers.Rpc.RpcException", "System.Threading.Tasks.TaskCanceledException")
        | where innermostMessage !contains "Non-Deterministic workflow detected: A previous execution of this orchestration scheduled an activity task with sequence ID 0"
        | where innermostMessage !contains "The operation was canceled."
        | where tostring(customDimensions["EventName"]) !in ("ProcessWorkItemFailed")
        | summarize exceptionCount = count() by type
        | order by exceptionCount desc
        QUERY
    },
    {
      name        = "timeseriesprocessor-error-trace-severity"
      description = "Triggers if there has been any traces with error severity in the past hour in the timeseriesprocessor function"
      severity    = 1
      frequency   = 60
      time_window = 60
      operator    = "GreaterThan"
      threshold   = 0
      query       = <<QUERY
        traces
        | where cloud_RoleName in ("${module.func_timeseriesprocessor.name}")
        | where severityLevel == 3
        | where tostring(customDimensions["EventName"]) !in ("OrchestrationProcessingFailure", "FunctionCompleted", "TaskActivityDispatcherError", "ProcessWorkItemFailed", "HealthCheckEnd", "PartitionManagerError")
        | where tostring(customDimensions["prop__reason"]) !contains ("DurableTask.Core.Exceptions.OrchestrationFailureException")
        | where tostring(customDimensions["prop__reason"]) !contains ("Microsoft.Azure.WebJobs.Host.FunctionInvocationException")
        | summarize eventCount = count() by tostring(customDimensions["EventName"])
        | order by eventCount desc
        QUERY
    }*/
  ]
}
