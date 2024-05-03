module "func_receiver" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app-elastic?ref=v14"

  name                                     = "api"
  project_name                             = var.domain_name_short
  environment_short                        = var.environment_short
  environment_instance                     = var.environment_instance
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  app_service_plan_id                      = module.func_service_plan.id
  vnet_integration_subnet_id               = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id               = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  dotnet_framework_version                 = "v8.0"
  use_dotnet_isolated_runtime              = true
  health_check_path                        = "/api/monitor/ready"
  ip_restrictions                          = var.ip_restrictions
  scm_ip_restrictions                      = var.ip_restrictions
  client_certificate_mode                  = "Optional"
  application_insights_connection_string   = data.azurerm_key_vault_secret.appi_shared_connection_string.value

  app_settings = local.default_func_api_app_settings

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

module "kvs_edi_api_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v14"
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
    DB_CONNECTION_STRING      = local.CONNECTION_STRING
    AZURE_STORAGE_ACCOUNT_URL = local.AZURE_STORAGE_ACCOUNT_URL

    # Logging
    "Logging__ApplicationInsights__LogLevel__Default"                     = local.LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT
    "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Edi"       = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_EDI
    "Logging__ApplicationInsights__LogLevel__Energinet.DataHub.Core"      = local.LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE

    # FeatureManagement
    FeatureManagement__UseMonthlyAmountPerChargeResultProduced = var.feature_management_use_monthly_amount_per_charge_result_produced
    FeatureManagement__UseAmountPerChargeResultProduced        = var.feature_management_use_amount_per_charge_result_produced
    FeatureManagement__UseRequestWholesaleSettlementReceiver   = var.feature_management_use_request_wholesale_settlement_receiver
    FeatureManagement__UseMessageDelegation                    = var.feature_management_use_message_delegation
    FeatureManagement__UsePeekMessages                         = var.feature_management_use_peek_messages
    FeatureManagement__UseRequestMessages                      = var.feature_management_use_request_messages
    FeatureManagement__UseEnergyResultProduced                 = var.feature_management_use_energy_result_produced

    # Service Bus
    ServiceBus__ManageConnectionString = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-manage-connection-string)"
    ServiceBus__ListenConnectionString = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-listen-connection-string)"
    ServiceBus__SendConnectionString   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-send-connection-string)"

    #Queue names
    EdiInbox__QueueName         = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbq-edi-inbox-messagequeue-name)"
    WholesaleInbox__QueueName   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbq-wholesale-inbox-messagequeue-name)"
    IncomingMessages__QueueName = azurerm_servicebus_queue.edi_incoming_messages_queue.name

    IntegrationEvents__TopicName        = local.INTEGRATION_EVENTS_TOPIC_NAME
    IntegrationEvents__SubscriptionName = module.sbtsub_edi_integration_event_listener.name
  }
}
