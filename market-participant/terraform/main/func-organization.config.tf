locals {
  func_organization = {
    app_settings = {
      FeatureManagement__EnabledOrganizationIdentityUpdateTrigger = var.enabled_organization_identity_update_trigger

      "Database:ConnectionString"                                 = local.MS_MARKET_PARTICIPANT_CONNECTION_STRING

      "SendGrid:ApiKey"                                           = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_api_key.name})",
      "SendGrid:SenderEmail"                                      = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_from_email.name})",
      "SendGrid:BccEmail"                                         = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_bcc_email.name})",

      "Environment:Description"                                   = local.ENV_DESC,

      "UserInvite:InviteFlowUrl"                                  = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=b2c-invitation-flow-uri)",

      "AzureB2c:Tenant"                                           = var.b2c_tenant
      "AzureB2c:SpnId"                                            = var.b2c_spn_id
      "AzureB2c:SpnSecret"                                        = var.b2c_spn_secret
      "AzureB2c:BackendObjectId"                                  = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-obj-id)"
      "AzureB2c:BackendSpnObjectId"                               = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-sp-id)"
      "AzureB2c:BackendId"                                        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-id)"

      "CvrRegister:BaseAddress"                                   = var.cvr_base_address
      "CvrRegister:Username"                                      = var.cvr_username
      "CvrRegister:Password"                                      = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_cvr_password.name})"

      "CvrUpdate:NotificationToEmail"                             = var.cvr_update_notification_to_email

      "BalanceResponsibleChanged:NotificationToEmail"             = var.balance_responsible_changed_notification_to_email

      "ServiceBus:SharedIntegrationEventTopic"                    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
      "ServiceBus:IntegrationEventSubscription"                   = module.sbtsub_market_participant_event_listener.name
      "ServiceBus:ConsumerConnectionString"                       = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-listen-connection-string)"
      "ServiceBus:ProducerConnectionString"                       = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-send-connection-string)",
      "ServiceBus:HealthConnectionString"                         = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sb-domain-relay-transceiver-connection-string)",
    }
  }
}
