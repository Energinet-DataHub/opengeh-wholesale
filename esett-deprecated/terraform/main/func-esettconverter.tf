module "func_converter" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  name                                      = "converter"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  vnet_integration_subnet_id                = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id                = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                       = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  ip_restriction_allow_ip_range             = var.hosted_deployagent_public_ip_range
  dotnet_framework_version                  = "v7.0"
  app_settings = {
    STORAGE_ACCOUNT_URL                                                   = "https://${module.stor_esett.name}.blob.core.windows.net"
    BLOB_FILES_ERROR_CONTAINER_NAME                                       = local.blob_files_error_container_name
    BLOB_FILES_RAW_CONTAINER_NAME                                         = local.blob_files_raw_container_name
    BLOB_FILES_ENRICHMENTS_CONTAINER_NAME                                 = local.blob_files_enrichments_container_name
    BLOB_FILES_CONVERTED_CONTAINER_NAME                                   = local.blob_files_converted_container_name
    BLOB_FILES_ACK_CONTAINER_NAME                                         = local.blob_files_ack_container_name
    "biztalk:acknowledgementMgaImbalance"                                 = "NBS-ACK-MGA-IMBALANCE-RESULTS"
    "biztalk:acknowledgementBrpChange"                                    = "NBS-ACK-RETAILER-BALANCE-RESPONSIBILITY"
    "Logging__LogLevel__Default"                                          = "Information"
    "Logging__LogLevel__Microsoft"                                        = "Warning"
    "Logging__ApplicationInsights__LogLevel__Default"                     = "Information"
    "Logging__ApplicationInsights__LogLevel__Microsoft"                   = "Warning"
    "Logging__ApplicationInsights__LogLevel__Microsoft.Hosting.Lifetime"  = "Information"
    CONNECTION_STRING_SHARED_BLOB                                         = module.stor_esett.primary_connection_string
    CONNECTION_STRING_DATABASE                                            = local.connection_string_database
  }
}
