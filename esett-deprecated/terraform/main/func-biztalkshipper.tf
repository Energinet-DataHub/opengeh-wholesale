module "func_biztalkshipper" {
  source                                        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  name                                          = "biztalkshipper"
  project_name                                  = var.domain_name_short
  environment_short                             = var.environment_short
  environment_instance                          = var.environment_instance
  resource_group_name                           = azurerm_resource_group.this.name
  location                                      = azurerm_resource_group.this.location
  vnet_integration_subnet_id                    = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id                    = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                           = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key      = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  ip_restrictions                               = var.ip_restrictions
  scm_ip_restrictions                           = var.ip_restrictions
  always_on                                     = true
  dotnet_framework_version                      = "v7.0"
  app_settings                                  = merge({
    "biztalk:RootUrl"                           = "https://datahub.dev01.biztalk.test.endk.local"
    "FeatureManagement__EnableBizTalkShipper"   = false
  }, local.default_biztalkshipper_settings)
}

locals {
  default_biztalkshipper_settings = {
    STORAGE_ACCOUNT_URL                                                   = "https://${module.stor_esett.name}.blob.core.windows.net"
    BLOB_FILES_ERROR_CONTAINER_NAME                                       = local.blob_files_error_container_name
    BLOB_FILES_SENT_CONTAINER_NAME                                        = local.blob_files_sent_container_name
    BLOB_FILES_CONVERTED_CONTAINER_NAME                                   = local.blob_files_converted_container_name
    BLOB_FILES_ACK_CONTAINER_NAME                                         = local.blob_files_ack_container_name
    "biztalk:Endpoint"                                                    = "/EL_DataHubService/IntegrationService.svc"
    "biztalk:businessTypeConsumption"                                     = "NBS-RECI"
    "biztalk:businessTypeProduction"                                      = "NBS-MEPI"
    "biztalk:businessTypeExchange"                                        = "NBS-MGXI"
    "Logging__LogLevel__Default"                                          = "Information"
    "Logging__LogLevel__Microsoft"                                        = "Warning"
    "Logging__ApplicationInsights__LogLevel__Default"                     = "Information"
    "Logging__ApplicationInsights__LogLevel__Microsoft"                   = "Warning"
    "Logging__ApplicationInsights__LogLevel__Microsoft.Hosting.Lifetime"  = "Information"
    CONNECTION_STRING_SHARED_BLOB                                         = module.stor_esett.primary_connection_string
    CONNECTION_STRING_DATABASE                                            = local.connection_string_database
    WEBSITE_LOAD_CERTIFICATES                                             = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=cert-esett-biztalk-thumbprint)"
  }
}
