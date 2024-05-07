# TODOs as part of step 2:
# - dhe-infrastructure/ui/app-bff: update ApiClientSettings__WholesaleOrchestrationsBaseUrl func-wholesale-orchestrationsdf-base-url
# - dh3-environments/wholesale-cd: update orchestrationsapi_baseaddress to
# - update healthchecks UI references
# - Create Sauron SQL migration script to delete old orchestrations function
module "func_orchestrationsdf" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app-elastic?ref=v14"

  name                                   = "orchestrationsdf"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  app_service_plan_id                    = module.func_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  dotnet_framework_version               = "v8.0"
  use_dotnet_isolated_runtime            = true
  is_durable_function                    = true
  health_check_path                      = "/api/monitor/ready"
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  role_assignments = [
    {
      // DataLake
      resource_id          = data.azurerm_key_vault_secret.st_data_lake_id.value
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      // Blob
      resource_id          = module.storage_settlement_reports.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      // Key Vault
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      // Shared Key Vault
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    }
  ]

  app_settings = {
    # Logging
    # => Azure Function Worker
    # Explicit override the default "Warning" level filter set by the Application Insights SDK.
    # Otherwise custom logging at "Information" level is not possible, even if we configure it on categories.
    # For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
    "Logging__ApplicationInsights__LogLevel__Default"                     = "Information"
    "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Wholesale" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_WHOLESALE
    "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core"      = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE

    # Storage (DataLake)
    STORAGE_CONTAINER_NAME = local.STORAGE_CONTAINER_NAME
    STORAGE_ACCOUNT_URI    = local.STORAGE_ACCOUNT_URI

    # Storage (Blob)
    "SettlementReportStorage__StorageAccountUri"    = local.BLOB_STORAGE_ACCOUNT_URI
    "SettlementReportStorage__StorageContainerName" = local.BLOB_CONTAINER_SETTLEMENTREPORTS_NAME

    # Service Bus
    "ServiceBus__ConnectionString"        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-transceiver-connection-string)"
    "IntegrationEvents__TopicName"        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
    "IntegrationEvents__SubscriptionName" = module.sbtsub_wholesale_integration_event_listener.name

    # Databricks
    WorkspaceToken = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-workspace-token)"
    WorkspaceUrl   = "https://${module.dbw.workspace_url}"
    WarehouseId    = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-databricks-sql-endpoint-id)"

    # Database
    "CONNECTIONSTRINGS__DB_CONNECTION_STRING" = local.DB_CONNECTION_STRING

    # Durable Functions Task Hub Name
    # See naming constraints: https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-task-hubs?tabs=csharp#task-hub-names
    "OrchestrationsTaskHubName" = "Wholesale01"
  }
}

module "kvs_func_orchestrationsdf_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v14"

  name         = "func-wholesale-orchestrationsdf-base-url"
  value        = "https://${module.func_orchestrationsdf.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
