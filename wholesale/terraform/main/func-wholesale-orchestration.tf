module "func_wholesale_orchestration" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  name                                   = "orchestration"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  app_service_plan_id                    = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  always_on                              = true
  dotnet_framework_version               = "v8.0"
  use_dotnet_isolated_runtime            = true
  health_check_path                      = "/api/monitor/ready"
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  role_assignments = [
    {
      // DataLake
      resource_id          = data.azurerm_key_vault_secret.st_data_lake_id.value
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]

  app_settings = {
    # Logging
    # => Azure Function Worker
    # Explicit override the default "Warning" level filter set by the Application Insights SDK.
    # Otherwise custom logging at "Information" level is not possible, even if we configure it on categories.
    # For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
    "Logging__ApplicationInsights__LogLevel__Default"                     = "Information"
    "Logging__ApplicationInsights__LogLevel__Energinet.Datahub.Wholesale" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_WHOLESALE
    "Logging__ApplicationInsights__LogLevel__Energinet.Datahub.Core"      = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE

    # Time zone
    TIME_ZONE = local.TIME_ZONE

    # Storage (DataLake)
    STORAGE_CONTAINER_NAME = local.STORAGE_CONTAINER_NAME
    STORAGE_ACCOUNT_URI    = local.STORAGE_ACCOUNT_URI

    # Service Bus
    SERVICE_BUS_SEND_CONNECTION_STRING       = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-send-connection-string)"
    SERVICE_BUS_TRANCEIVER_CONNECTION_STRING = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-transceiver-connection-string)"
    INTEGRATIONEVENTS_TOPIC_NAME             = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
    INTEGRATIONEVENTS_SUBSCRIPTION_NAME      = module.sbtsub_wholesale_integration_event_listener.name
    EDI_INBOX_MESSAGE_QUEUE_NAME             = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbq-edi-inbox-messagequeue-name)"
    WHOLESALE_INBOX_MESSAGE_QUEUE_NAME       = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbq-wholesale-inbox-messagequeue-name)"

    # Databricks
    WorkspaceToken = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-workspace-token)"
    WorkspaceUrl   = "https://${module.dbw.workspace_url}"
    WarehouseId    = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-databricks-sql-endpoint-id)"

    # Database
    "CONNECTIONSTRINGS__DB_CONNECTION_STRING" = local.DB_CONNECTION_STRING
  }
}
