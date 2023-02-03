module "func_receiver" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v10"

  name                                      = "api"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  app_service_plan_id                       = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  log_analytics_workspace_id                = data.azurerm_key_vault_secret.log_shared_id.value
  vnet_integration_subnet_id                = data.azurerm_key_vault_secret.snet_vnet_integrations_id.value
  private_endpoint_subnet_id                = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  always_on                                 = true
  dotnet_framework_version                  = "6"
  use_dotnet_isolated_runtime               = true
  health_check_path                         = "/api/monitor/ready"
  app_settings                              = {
    # Shared resources logging
    REQUEST_RESPONSE_LOGGING_CONNECTION_STRING                    = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketoplogs-primary-connection-string)",
    REQUEST_RESPONSE_LOGGING_CONTAINER_NAME                       = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=st-marketoplogs-container-name)",
    B2C_TENANT_ID                                                 = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=b2c-tenant-id)",
    BACKEND_SERVICE_APP_ID                                        = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-service-app-id)",
    # Endregion: Default Values
    DB_CONNECTION_STRING                                          = local.MS_MARKETROLES_CONNECTION_STRING
    INCOMING_CHANGE_OF_SUPPLIER_MESSAGE_QUEUE_NAME                = module.sbq_incoming_change_supplier_messagequeue.name
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_LISTENER       = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-listen-connection-string)"
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_MANAGE         = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-manage-connection-string)"
    SERVICE_BUS_CONNECTION_STRING_FOR_DOMAIN_RELAY_SEND           = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-send-connection-string)"
    FEATUREFLAG_ACTORMESSAGEQUEUE                                 = true
    PERFORMANCE_TEST_ENABLED                                      = var.performance_test_enabled
    INTEGRATION_EVENTS_TOPIC_NAME                                 = local.INTEGRATION_EVENTS_TOPIC_NAME
    BALANCE_FIXING_RESULT_AVAILABLE_EVENT_SUBSCRIPTION_NAME       = module.sbtsub_wholesale_process_completed_event_listener.name
    AGGREGATION_RESULTS_API_URI                                   = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=app-wholesale-api-base-url)"
  }
}
