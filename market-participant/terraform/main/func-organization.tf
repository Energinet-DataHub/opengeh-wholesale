module "func_organization" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v14"

  name                                   = "organization"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                    = module.webapp_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  health_check_path                      = "/api/monitor/ready"
  always_on                              = true
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  health_check_alert = {
    action_group_id = data.azurerm_key_vault_secret.primary_action_group_id.value
    enabled         = var.enable_health_check_alerts
  }
  dotnet_framework_version    = "v8.0"
  use_dotnet_isolated_runtime = true
  app_settings                = local.default_organization_app_settings

  role_assignments = [
    {
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    }
  ]
}

locals {
  default_organization_app_settings = {
    SQL_MP_DB_CONNECTION_STRING                       = local.MS_MARKET_PARTICIPANT_CONNECTION_STRING
    SERVICE_BUS_CONNECTION_STRING                     = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-send-connection-string)",
    SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-transceiver-connection-string)",
    SBT_MARKET_PARTICIPANT_CHANGED_NAME               = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)",
    SEND_GRID_APIKEY                                  = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_api_key.name})",
    SENDER_EMAIL                                      = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_from_email.name})",
    BCC_EMAIL                                         = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_bcc_email.name})",
    USER_INVITE_FLOW                                  = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=b2c-invitation-flow-uri)",
    AZURE_B2C_TENANT                                  = var.b2c_tenant
    AZURE_B2C_SPN_ID                                  = var.b2c_spn_id
    AZURE_B2C_SPN_SECRET                              = var.b2c_spn_secret
    AZURE_B2C_BACKEND_OBJECT_ID                       = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-obj-id)"
    AZURE_B2C_BACKEND_SPN_OBJECT_ID                   = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-sp-id)"
    AZURE_B2C_BACKEND_ID                              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-id)"
    ENVIRONMENT_DESC                                  = local.ENV_DESC
    CVR_BASE_ADDRESS                                  = var.cvr_base_address
    CVR_USERNAME                                      = var.cvr_username
    CVR_PASSWORD                                      = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_cvr_password.name})"
    CVR_ORG_UPDATED_NOTIFICATION_TO_EMAIL             = var.cvr_update_notification_to_email
    BALANCE_RESPONSIBLE_CHANGED_NOTIFICATION_TO_EMAIL = var.balance_responsible_changed_notification_to_email

    "ConsumeServiceBusSettings:ConnectionString"             = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-listen-connection-string)"
    "ConsumeServiceBusSettings:SharedIntegrationEventTopic"  = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
    "ConsumeServiceBusSettings:IntegrationEventSubscription" = module.sbtsub_market_participant_event_listener.name

    FeatureManagement__EnabledOrganizationIdentityUpdateTrigger = var.enabled_organization_identity_update_trigger

    "Database:ConnectionString"                               = local.MS_MARKET_PARTICIPANT_CONNECTION_STRING

    "SendGrid:ApiKey"                                         = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_api_key.name})",
    "SendGrid:SenderEmail"                                    = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_from_email.name})",
    "SendGrid:BccEmail"                                       = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_bcc_email.name})",

    "Environment:Description"                                 = local.ENV_DESC,

    "UserInvite.InviteFlowUrl"                                = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=b2c-invitation-flow-uri)",

    "AzureB2c:Tenant"                                         = var.b2c_tenant
    "AzureB2c:SpnId"                                          = var.b2c_spn_id
    "AzureB2c:SpnSecret"                                      = var.b2c_spn_secret
    "AzureB2c:BackendObjectId"                                = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-obj-id)"
    "AzureB2c:BackendSpnObjectId"                             = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-sp-id)"
    "AzureB2c:BackendId"                                      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-id)"

    "CvrRegister:BaseAddress"                                 = var.cvr_base_address
    "CvrRegister:Username"                                    = var.cvr_username
    "CvrRegister:Password"                                    = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_cvr_password.name})"

    "CvrUpdate:NotificationToEmail"                           = var.cvr_update_notification_to_email

    "BalanceResponsibleChanged:NotificationToEmail"           = var.balance_responsible_changed_notification_to_email

    "ServiceBus:SharedIntegrationEventTopic"                  = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
    "ServiceBus:IntegrationEventSubscription"                 = module.sbtsub_market_participant_event_listener.name
    "ServiceBus:ConsumerConnectionString"                     = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-listen-connection-string)"
    "ServiceBus:ProducerConnectionString"                     = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-send-connection-string)",
    "ServiceBus:HealthConnectionString"                       = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-transceiver-connection-string)",
  }
}
