module "app_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v14"

  name                                   = "api"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id             = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                    = module.webapp_service_plan.id
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  health_check_path                      = "/monitor/ready"
  health_check_alert_action_group_id     = data.azurerm_key_vault_secret.primary_action_group_id.value
  health_check_alert_enabled             = var.enable_health_check_alerts
  dotnet_framework_version               = "v8.0"
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  app_settings                           = local.app_api.app_settings

  role_assignments = [
    {
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      resource_id          = module.kv_internal.id
      role_definition_name = "Key Vault Crypto User"
    },
    {
      resource_id          = module.kv_dh2_certificates.id
      role_definition_name = "Key Vault Secrets Officer"
    },
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    }
  ]
}

module "kvs_app_markpart_api_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "app-markpart-api-base-url"
  value        = "https://${module.app_api.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

module "kvs_backend_api_open_id_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "api-backend-open-id-url"
  value        = "https://${module.app_api.default_hostname}/.well-known/openid-configuration"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

locals {
  default_api_app_settings = {
    "UserAuthentication:MitIdExternalMetadataAddress" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=mitid-frontend-open-id-url)"
    "UserAuthentication:ExternalMetadataAddress"      = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=frontend-open-id-url)"
    "UserAuthentication:InternalMetadataAddress"      = "https://app-api-${var.domain_name_short}-${var.environment_short}-we-${var.environment_instance}.azurewebsites.net/.well-known/openid-configuration"
    "UserAuthentication:BackendBffAppId"              = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-bff-app-id)"

    "Database:ConnectionString" = local.MS_MARKET_PARTICIPANT_CONNECTION_STRING

    "AzureB2c:Tenant"             = var.b2c_tenant
    "AzureB2c:SpnId"              = var.b2c_spn_id
    "AzureB2c:SpnSecret"          = var.b2c_spn_secret
    "AzureB2c:BackendObjectId"    = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-obj-id)"
    "AzureB2c:BackendSpnObjectId" = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-sp-id)"
    "AzureB2c:BackendId"          = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=backend-b2b-app-id)"

    "KeyVault:TokenSignKeyVault"    = module.kv_internal.vault_uri
    "KeyVault:TokenSignKeyName"     = azurerm_key_vault_key.token_sign.name
    "KeyVault:CertificatesKeyVault" = module.kv_dh2_certificates.vault_uri

    "CvrRegister:BaseAddress" = var.cvr_base_address
    "CvrRegister:Username"    = var.cvr_username
    "CvrRegister:Password"    = "@Microsoft.KeyVault(VaultName=${module.kv_internal.name};SecretName=${module.kvs_cvr_password.name})"
  }
}
