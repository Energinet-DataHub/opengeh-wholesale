module "func_receiver" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v12"

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
  ip_restriction_allow_ip_range            = var.hosted_deployagent_public_ip_range

  app_settings = {
    # Shared resources logging
    REQUEST_RESPONSE_LOGGING_CONNECTION_STRING = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketoplogs-primary-connection-string)",
    REQUEST_RESPONSE_LOGGING_CONTAINER_NAME    = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketoplogs-container-name)",
    B2C_TENANT_ID                              = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=b2c-tenant-id)",
    BACKEND_SERVICE_APP_ID                     = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-b2b-app-id)",
    # Endregion: Default Values
    DB_CONNECTION_STRING                                    = local.CONNECTION_STRING
    EDI_INBOX_MESSAGE_QUEUE_NAME                            = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-edi-inbox-messagequeue-name)"
    WHOLESALE_INBOX_MESSAGE_QUEUE_NAME                      = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbq-wholesale-inbox-messagequeue-name)"
    INCOMING_CHANGE_OF_SUPPLIER_MESSAGE_QUEUE_NAME          = module.sbq_incoming_change_supplier_messagequeue.name
    INCOMING_AGGREGATED_MEASURE_DATA_QUEUE_NAME             = module.sbq_incoming_aggregated_measure_data_messagequeue.name
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_LISTENER = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-listen-connection-string)"
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_MANAGE   = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-manage-connection-string)"
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_SEND     = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-send-connection-string)"
    FEATUREFLAG_ACTORMESSAGEQUEUE                           = true
    ALLOW_TEST_TOKENS                                       = var.allow_test_tokens
    INTEGRATION_EVENTS_TOPIC_NAME                           = local.INTEGRATION_EVENTS_TOPIC_NAME
    BALANCE_FIXING_RESULT_AVAILABLE_EVENT_SUBSCRIPTION_NAME = module.sbtsub_wholesale_process_completed_event_listener.name
  }
}

module "kvs_edi_api_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "func-edi-api-base-url"
  value        = "https://${module.func_receiver.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
