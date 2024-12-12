module "func_orchestrations" {
  app_settings = merge(local.func_orchestrations.app_settings, {
    # Timer triggers
    # Override settings to enable them i development/tests environments
    "AzureWebJobs.StartScheduledOrchestrationInstances.Disabled" = false
    "AzureWebJobs.PerformRecurringPlanning.Disabled"             = false
  })
}
