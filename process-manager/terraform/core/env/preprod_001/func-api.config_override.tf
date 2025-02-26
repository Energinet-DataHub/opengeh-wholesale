module "func_api" {
  app_settings = merge(local.func_api.app_settings, {
    # Timer triggers
    # Override settings to enable them i development/tests environments
    "AzureWebJobs.StartScheduledOrchestrationInstances.Disabled" = false
  })
}
