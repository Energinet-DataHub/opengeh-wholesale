module "app_webapi" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v12"

  name                                     = "webapi"
  project_name                             = var.domain_name_short
  environment_short                        = var.environment_short
  environment_instance                     = var.environment_instance
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  vnet_integration_subnet_id               = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id               = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                      = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  health_check_path                        = "/monitor/ready"
  health_check_alert_action_group_id       = data.azurerm_key_vault_secret.primary_action_group_id.value
  health_check_alert_enabled               = var.enable_health_check_alerts
  dotnet_framework_version                 = "v7.0"
  ip_restriction_allow_ip_range            = var.hosted_deployagent_public_ip_range
  app_settings = {
    "JwtBearerSettings:ExternalOpenIdUrl"  = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=frontend-open-id-url)"
    "JwtBearerSettings:InternalOpenIdUrl"  = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-open-id-url)"
    "JwtBearerSettings:BackendBffAppId"    = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=backend-bff-app-id)"
    "DatabaseSettings:ConnectionString"    = local.MS_ESETT_EXCHANGE_CONNECTION_STRING
    "BlobStorageSettings:AccountUri"       = local.ESETT_DOCUMENT_STORAGE_ACCOUNT_URI
    "BlobStorageSettings:ContainerName"    = local.ESETT_DOCUMENT_STORAGE_CONTAINER_NAME
  }
  role_assignments = [
    {
      resource_id          = module.storage_esett_documents.id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
}

module "kvs_app_esett_webapi_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "app-esett-webapi-base-url"
  value        = "https://${module.app_webapi.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
