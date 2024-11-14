locals {
  func_process_manager_api = {
    app_settings = {
      # Logging
      "Logging__ApplicationInsights__LogLevel__Default"                           = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.ProcessManager"  = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core"            = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE

      # Durable Functions Task Hub Name
      # See naming constraints: https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-task-hubs?tabs=csharp#task-hub-names
      # "ProcessManagerTaskHubName" must match the "ProcessManagerTaskHubName" value in func_process_manager_orchestrations
      "ProcessManagerTaskHubName"             = "ProcessManager01"
      "ProcessManagerStorageConnectionString" = "${module.st_process_manager.primary_connection_string}"

      # Database
      "ProcessManager__SqlDatabaseConnectionString": "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_process_manager.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    }
  }
}
