module "func_entrypoint_marketparticipant" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  name                                   = "organization"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                    = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  always_on                              = true
  health_check_path                      = "/api/monitor/ready"
  health_check_alert = {
    action_group_id = data.azurerm_key_vault_secret.primary_action_group_id.value
    enabled         = var.enable_health_check_alerts
  }
  dotnet_framework_version    = "v8.0"
  use_dotnet_isolated_runtime = true

  app_settings = {
    SQL_MP_DB_CONNECTION_STRING                       = local.MS_MARKET_PARTICIPANT_CONNECTION_STRING
    SERVICE_BUS_CONNECTION_STRING                     = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-send-connection-string)",
    SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-manage-connection-string)",
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

    FeatureManagement__EnabledOrganizationIdentityUpdateTrigger = var.enabled_organization_identitiy_update_trigger
  }

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
