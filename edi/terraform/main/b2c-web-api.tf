module "b2c_web_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v14"

  name                                     = "b2cwebapi"
  project_name                             = var.domain_name_short
  environment_short                        = var.environment_short
  environment_instance                     = var.environment_instance
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  app_service_plan_id                      = module.webapp_service_plan.id
  vnet_integration_subnet_id               = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id               = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  dotnet_framework_version                 = "v8.0"
  health_check_path                        = "/monitor/ready"
  health_check_alert_action_group_id       = data.azurerm_key_vault_secret.primary_action_group_id.value
  health_check_alert_enabled               = true
  ip_restrictions                          = var.ip_restrictions
  scm_ip_restrictions                      = var.ip_restrictions
  application_insights_connection_string   = data.azurerm_key_vault_secret.appi_shared_connection_string.value

  app_settings = {
    DB_CONNECTION_STRING      = local.CONNECTION_STRING
    AZURE_STORAGE_ACCOUNT_URL = local.AZURE_STORAGE_ACCOUNT_URL

    # Authentication
    UserAuthentication__ExternalMetadataAddress      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
    UserAuthentication__InternalMetadataAddress      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=api-backend-open-id-url)"
    UserAuthentication__BackendBffAppId              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"
    UserAuthentication__MitIdExternalMetadataAddress = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"


    # Service Bus
    ServiceBus__ManageConnectionString = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-manage-connection-string)"
    ServiceBus__ListenConnectionString = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-listen-connection-string)"
    ServiceBus__SendConnectionString   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-send-connection-string)"

    # Queue names
    IncomingMessages__QueueName = azurerm_servicebus_queue.edi_incoming_messages_queue.name

  # Logging
  "Logging__ApplicationInsights__LogLevel__Default"                = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
  "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Edi"  = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_EDI
  "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core" = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE
}

  # Role assigments is needed to connect to the storage account (st_documents) using URI
  role_assignments = [
    {
      resource_id          = module.st_documents.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    }
  ]
}

module "kvs_app_edi_b2cwebapi_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v14"

  name         = "app-edi-b2cwebapi-base-url"
  value        = "https://${module.b2c_web_api.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
