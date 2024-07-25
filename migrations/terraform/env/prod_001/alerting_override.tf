module "monitor_action_group_mig" {
  query_alerts_list = [
    {
      name        = "alert-dropzoneunzipper-exception-${local.resources_suffix}"
      description = "Check every 1 hour if there has been exceptions in the past hour"
      severity    = 1
      frequency   = 60
      time_window = 60
      operator    = "GreaterThan"
      threshold   = 0
      query       = <<QUERY
        exceptions
        | where cloud_RoleName in ("${module.func_dropzoneunzipper.name}")
        | summarize count() by bin(timestamp, 1h)
        QUERY
    },
    {
      name        = "alert-dropzoneunzipper-error-trace-severity-${local.resources_suffix}"
      description = "Check every 1 hour if there has been any traces with error severity in the past hour"
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
        | summarize count() by bin(timestamp, 1h)
        QUERY
    },
    {
      name        = "alert-timeseriessynchronization-exception-${local.resources_suffix}"
      description = "Check every 1 hour if there has been exceptions in the past hour"
      severity    = 1
      frequency   = 60
      time_window = 60
      operator    = "GreaterThan"
      threshold   = 0
      query       = <<QUERY
        exceptions
        | where cloud_RoleName in ("${module.func_timeseriessynchronization.name}")
        | summarize count() by bin(timestamp, 1h)
        QUERY
    },
    {
      name        = "alert-timeseriessynchronization-error-trace-severity-${local.resources_suffix}"
      description = "Check every 1 hour if there has been any traces with error severity in the past hour"
      severity    = 1
      frequency   = 60
      time_window = 60
      operator    = "GreaterThan"
      threshold   = 0
      query       = <<QUERY
        traces
        | where cloud_RoleName in ("${module.func_timeseriessynchronization.name}")
        | where severityLevel == 3
        | where tostring(customDimensions["EventName"]) !in ("OrchestrationProcessingFailure", "FunctionCompleted", "TaskActivityDispatcherError", "ProcessWorkItemFailed")
        | summarize count() by bin(timestamp, 1h)
        QUERY
    }
  ]
}
