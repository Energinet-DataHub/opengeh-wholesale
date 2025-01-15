locals {
  func_api = {
    app_settings = {
      # Logging
      "Logging__ApplicationInsights__LogLevel__Default"                          = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core"           = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.ProcessManager" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_PROCESS_MANAGER

      # Durable Functions Task Hub Name
      # See naming constraints: https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-task-hubs?tabs=csharp#task-hub-names
      # "ProcessManagerTaskHubName" must match the "ProcessManagerTaskHubName" value in func_orchestrations_process_manager
      "ProcessManagerTaskHubName"             = local.OrchestrationsTaskHubName
      "ProcessManagerStorageConnectionString" = module.st_taskhub.primary_connection_string

      # Database
      "ProcessManager__SqlDatabaseConnectionString" = local.DatabaseConnectionString

      # NotifyOrchestrationInstance subscription
      "ServiceBus__FullyQualifiedNamespace"     = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-namespace-endpoint)"
      "NotifyOrchestrationInstance__TopicName"  = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-processmanager-name)"
      "NotifyOrchestrationInstance__NotifyOrchestrationInstanceSubscriptionName" = module.sbtsub_pm_notify_orchestration.name

      # Timer triggers
      # IMPORTANT: Override settings to enable them i development/tests environments
      "AzureWebJobs.StartScheduledOrchestrationInstances.Disabled" = true
      "AzureWebJobs.PerformRecurringPlanning.Disabled"             = true
    }
  }
}
