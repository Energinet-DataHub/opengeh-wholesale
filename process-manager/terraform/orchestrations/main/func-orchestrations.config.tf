
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

      # Authentication
      "Auth__ApplicationIdUri" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=processmanager-application-id-uri)"
      "Auth__Issuer"           = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"

      # Database
      "ProcessManager__SqlDatabaseConnectionString" = data.azurerm_key_vault_secret.mssqldb_connection_string.value

      # PM Topic subscriptions
      "ServiceBus__FullyQualifiedNamespace" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-namespace-endpoint)"

      "ProcessManagerTopic__TopicName"                                = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-processmanager-name)"
      "ProcessManagerTopic__Brs026SubscriptionName"                   = module.sbtsub_pm_brs_026.name
      "ProcessManagerTopic__Brs028SubscriptionName"                   = module.sbtsub_pm_brs_028.name
      "ProcessManagerTopic__Brs021ForwardMeteredDataSubscriptionName" = module.sbtsub_pm_brs_021_forward_metered_data.name

      # BRS-021 (FMD) topic subscriptions
      "Brs021ForwardMeteredDataTopic__StartTopicName"         = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-brs021forwardmetereddatastart-name)"
      "Brs021ForwardMeteredDataTopic__NotifyTopicName"        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-brs021forwardmetereddatanotify-name)"
      "Brs021ForwardMeteredDataTopic__StartSubscriptionName"  = module.sbtsub_pm_brs_021_forward_metered_data_start.name
      "Brs021ForwardMeteredDataTopic__NotifySubscriptionName" = module.sbtsub_pm_brs_021_forward_metered_data_notify.name

      # Subsystem Topics
      "EdiTopic__Name"              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-edi-name)"
      "IntegrationEventTopic__Name" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"

      # Databricks workspaces
      "WholesaleWorkspace__BaseUrl"    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-wholesale-workspace-url)"
      "WholesaleWorkspace__Token"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-wholesale-workspace-token)"
      "MeasurementsWorkspace__BaseUrl" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-measurements-workspace-url)"
      "MeasurementsWorkspace__Token"   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=dbw-measurements-workspace-token)"

      # Measurements EventHub
      "MeasurementsEventHub__NamespaceName" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=evhns-measurements-name)"
      "MeasurementsEventHub__EventHubName"  = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=evh-measurement-transactions-name)"

      # Electricity Market client
      "ElectricityMarketClientOptions__BaseUrl"           = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mp-data-api-base-url)"
      "ElectricityMarketClientOptions__ApplicationIdUri"  = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=electricitymarket-application-id-uri)"

      # Wholesale database
      "WholesaleDatabase__SqlDatabaseConnectionString" = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${local.wholesale_db_name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"

      # FeatureManagement
      FeatureManagement__SilentMode = var.feature_management_silent_mode
    }
  }
}
