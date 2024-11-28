
locals {
  func_orchestrations = {
    app_settings = {
      # Logging
      "Logging__ApplicationInsights__LogLevel__Default"                          = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core"           = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.ProcessManager" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_PROCESS_MANAGER

      # Durable Functions Task Hub Name
      # See naming constraints: https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-task-hubs?tabs=csharp#task-hub-names
      # "ProcessManagerTaskHubName" must match the "ProcessManagerTaskHubName" value in func_process_manager_api
      # "ProcessManagerTaskHubName" and "ProcessManagerStorageConnectionString" must match the names given in the host.json file for the app
      "ProcessManagerTaskHubName"             = local.OrchestrationsTaskHubName
      "ProcessManagerStorageConnectionString" = module.st_taskhub.primary_connection_string

      # Database
      "ProcessManager__SqlDatabaseConnectionString" = local.DatabaseConnectionString
    }
  }
}
