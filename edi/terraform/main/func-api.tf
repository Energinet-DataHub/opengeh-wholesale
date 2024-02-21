module "func_receiver" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  name                                     = "api"
  project_name                             = var.domain_name_short
  environment_short                        = var.environment_short
  environment_instance                     = var.environment_instance
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  app_service_plan_id                      = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  vnet_integration_subnet_id               = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id               = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  always_on                                = true
  dotnet_framework_version                 = "v7.0"
  use_dotnet_isolated_runtime              = true
  health_check_path                        = "/api/monitor/ready"
  ip_restrictions                          = var.ip_restrictions
  scm_ip_restrictions                      = var.ip_restrictions
  client_certificate_mode                  = "Optional"

  app_settings = local.default_func_api_app_settings

  # Role assigments is needed to connect to the storage account (st_documents) using URI
  role_assignments = [
    {
      resource_id          = module.st_documents.id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
}

module "kvs_edi_api_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "func-edi-api-base-url"
  value        = "https://${module.func_receiver.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

locals {
  default_func_api_app_settings = {
    # Shared resources logging
    REQUEST_RESPONSE_LOGGING_CONNECTION_STRING = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=st-marketoplogs-primary-connection-string)",
    REQUEST_RESPONSE_LOGGING_CONTAINER_NAME    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=st-marketoplogs-container-name)",
    B2C_TENANT_ID                              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=b2c-tenant-id)",
    BACKEND_SERVICE_APP_ID                     = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-id)",
    # Endregion: Default Values
    DB_CONNECTION_STRING                                        = local.CONNECTION_STRING
    AZURE_STORAGE_ACCOUNT_URL                                   = local.AZURE_STORAGE_ACCOUNT_URL
    EDI_INBOX_MESSAGE_QUEUE_NAME                                = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbq-edi-inbox-messagequeue-name)"
    WHOLESALE_INBOX_MESSAGE_QUEUE_NAME                          = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbq-wholesale-inbox-messagequeue-name)"
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_LISTENER     = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-listen-connection-string)"
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_MANAGE       = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-manage-connection-string)"
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_SEND         = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-send-connection-string)"
    INCOMING_MESSAGES_QUEUE_NAME                                = azurerm_servicebus_queue.edi_incoming_messages_queue.name
    FEATUREFLAG_ACTORMESSAGEQUEUE                               = true
    INTEGRATION_EVENTS_TOPIC_NAME                               = local.INTEGRATION_EVENTS_TOPIC_NAME
    INTEGRATION_EVENTS_SUBSCRIPTION_NAME                        = module.sbtsub_edi_integration_event_listener.name
    FeatureManagement__UseMonthlyAmountPerChargeResultProduced  = var.feature_management_use_monthly_amount_per_charge_result_produced
    FeatureManagement__UseAmountPerChargeResultProduced         = var.feature_management_use_amount_per_charge_result_produced
  }
}
