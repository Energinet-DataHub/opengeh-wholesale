
locals {
  func_orchestrations = {
    app_settings = {
      # Logging
      "Logging__ApplicationInsights__LogLevel__Default"                          = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core"           = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE
      "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.ProcessManager" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_PROCESS_MANAGER

      # Durable Functions Task Hub Name
      # See naming constraints: https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-task-hubs?tabs=csharp#task-hub-names
      # "ProcessManagerTaskHubName" must match the "ProcessManagerTaskHubName" value in func_orchestrations
      # "ProcessManagerTaskHubName" and "ProcessManagerStorageConnectionString" must match the names given in the host.json file for the app
      "ProcessManagerTaskHubName"             = local.OrchestrationsTaskHubName
      "ProcessManagerStorageConnectionString" = data.azurerm_key_vault_secret.st_taskhub_primary_connection_string.value

      # Database
      "ProcessManager__SqlDatabaseConnectionString" = data.azurerm_key_vault_secret.mssqldb_connection_string.value

      # PM Topic subscriptions
      "ServiceBus__FullyQualifiedNamespace" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-namespace-endpoint)"

      "ProcessManagerTopic__TopicName"                                = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-processmanager-name)"
      "ProcessManagerTopic__Brs026SubscriptionName"                   = module.sbtsub_pm_brs_026.name
      "ProcessManagerTopic__Brs028SubscriptionName"                   = module.sbtsub_pm_brs_028.name
      "ProcessManagerTopic__Brs021ForwardMeteredDataSubscriptionName" = module.sbtsub_pm_brs_021_forward_metered_data.name

      "EdiTopic__Name" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-edi-name)"

      # Databricks workspaces
      "WholesaleWorkspace__BaseUrl"    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-wholesale-workspace-url)"
      "WholesaleWorkspace__Token"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-wholesale-workspace-token)"
      "MeasurementsWorkspace__BaseUrl" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-measurements-workspace-url)"
      "MeasurementsWorkspace__Token"   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-measurements-workspace-token)"

      # Measurements EventHub
      "MeasurementsEventHub__NamespaceName"   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=evhns-measurements-name)"
      "MeasurementsEventHub__EventHubName"    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=evh-measurement-transactions-name)"
    }
  }
}
