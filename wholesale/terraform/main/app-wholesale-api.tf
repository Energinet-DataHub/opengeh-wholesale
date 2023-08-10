module "app_wholesale_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v12"

  name                                     = "webapi"
  project_name                             = var.domain_name_short
  environment_short                        = var.environment_short
  environment_instance                     = var.environment_instance
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  app_service_plan_id                      = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  vnet_integration_subnet_id               = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id               = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  health_check_path                        = "/monitor/ready"
  health_check_alert_action_group_id       = data.azurerm_key_vault_secret.primary_action_group_id.value
  health_check_alert_enabled               = var.enable_health_check_alerts
  ip_restriction_allow_ip_range            = var.hosted_deployagent_public_ip_range
  role_assignments = [
    {
      resource_id          = data.azurerm_key_vault_secret.st_shared_data_lake_id.value
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]

  # Ensure that IHostedServices are not terminated due to unloading of the application in periods with no traffic
  always_on = true

  app_settings = {
    TIME_ZONE            = local.TIME_ZONE
    EXTERNAL_OPEN_ID_URL = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=frontend-open-id-url)"
    INTERNAL_OPEN_ID_URL = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-open-id-url)"
    BACKEND_BFF_APP_ID   = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-bff-app-id)"

    # Storage
    STORAGE_CONTAINER_NAME = local.STORAGE_CONTAINER_NAME
    STORAGE_ACCOUNT_URI    = local.STORAGE_ACCOUNT_URI

    # Service Bus
    SERVICE_BUS_SEND_CONNECTION_STRING   = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-send-connection-string)"
    SERVICE_BUS_LISTEN_CONNECTION_STRING = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-listen-connection-string)"
    SERVICE_BUS_MANAGE_CONNECTION_STRING = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-manage-connection-string)"
    INTEGRATIONEVENTS_TOPIC_NAME         = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-sharedres-integrationevent-received-name)"

    # Databricks
    DATABRICKS_WORKSPACE_TOKEN = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=dbw-shared-workspace-token)"
    DATABRICKS_WORKSPACE_URL   = "https://${data.azurerm_key_vault_secret.dbw_databricks_workspace_url.value}"
    DATABRICKS_WAREHOUSE_ID    = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=dbw-databricks-sql-endpoint-id)"

    # Logging
    "Logging__ApplicationInsights__LogLevel__Default"                     = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
    "Logging__ApplicationInsights__LogLevel__Energinet.Datahub.Wholesale" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_WHOLESALE
    "Logging__ApplicationInsights__LogLevel__Energinet.Datahub.Core"      = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE
  }

  connection_strings = [
    {
      name  = "DB_CONNECTION_STRING"
      type  = "SQLAzure"
      value = local.DB_CONNECTION_STRING
    }
  ]
}

module "kvs_app_wholesale_api_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "app-wholesale-api-base-url"
  value        = "https://${module.app_wholesale_api.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
