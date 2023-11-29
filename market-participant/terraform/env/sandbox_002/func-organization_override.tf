module "func_entrypoint_marketparticipant" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  app_settings = {
    SQL_MP_DB_CONNECTION_STRING                = local.MS_MARKET_PARTICIPANT_CONNECTION_STRING
    SERVICE_BUS_CONNECTION_STRING              = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-send-connection-string)",
    SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-manage-connection-string)",
    SBT_MARKET_PARTICIPANT_CHANGED_NAME        = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-shres-integrationevent-received-name)",
    SEND_GRID_APIKEY                           = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_api_key.name})",
    USER_INVITE_FROM_EMAIL                     = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_from_email.name})",
    USER_INVITE_BCC_EMAIL                      = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_sendgrid_bcc_email.name})",
    USER_INVITE_FLOW                           = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=b2c-invitation-flow-uri)",
    AZURE_B2C_TENANT                           = var.b2c_tenant
    AZURE_B2C_SPN_ID                           = var.b2c_spn_id
    AZURE_B2C_SPN_SECRET                       = var.b2c_spn_secret
    AZURE_B2C_BACKEND_OBJECT_ID                = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-b2b-app-obj-id)"
    AZURE_B2C_BACKEND_SPN_OBJECT_ID            = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-b2b-app-sp-id)"
    AZURE_B2C_BACKEND_ID                       = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-b2b-app-id)"
  }
}
