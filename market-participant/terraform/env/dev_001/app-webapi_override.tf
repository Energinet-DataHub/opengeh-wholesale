module "app_webapi" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v13"

  app_settings = {
    EXTERNAL_OPEN_ID_URL                       = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=frontend-open-id-url)"
    INTERNAL_OPEN_ID_URL                       = "https://app-webapi-${var.domain_name_short}-${var.environment_short}-${azurerm_resource_group.this.location}-${var.environment_instance}.azurewebsites.net/.well-known/openid-configuration"
    BACKEND_BFF_APP_ID                         = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-bff-app-id)"
    SQL_MP_DB_CONNECTION_STRING                = local.MS_MARKET_PARTICIPANT_CONNECTION_STRING
    SERVICE_BUS_CONNECTION_STRING              = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-send-connection-string)"
    SERVICE_BUS_HEALTH_CHECK_CONNECTION_STRING = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sb-domain-relay-manage-connection-string)"
    SBT_MARKET_PARTICIPANT_CHANGED_NAME        = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-shres-integrationevent-received-name)"
    AZURE_B2C_TENANT                           = var.b2c_tenant
    AZURE_B2C_SPN_ID                           = var.b2c_spn_id
    AZURE_B2C_SPN_SECRET                       = var.b2c_spn_secret
    AZURE_B2C_BACKEND_OBJECT_ID                = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-b2b-app-obj-id)"
    AZURE_B2C_BACKEND_SPN_OBJECT_ID            = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-b2b-app-sp-id)"
    AZURE_B2C_BACKEND_ID                       = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-b2b-app-id)"
    TOKEN_KEY_VAULT                            = module.kv_internal.vault_uri
    TOKEN_KEY_NAME                             = azurerm_key_vault_key.token_sign.name
  }
}
