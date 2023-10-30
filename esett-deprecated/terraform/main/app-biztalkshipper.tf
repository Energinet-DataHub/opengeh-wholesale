module "app_biztalkshipper" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v13"

  name                                      = "biztalkshipper"
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
  dotnet_framework_version                  = "v6.0"
  app_settings                              = merge({
    "biztalk:RootUrl"                       = "https://datahub.dev01.biztalk.test.endk.local"
  }, local.default_biztalkshipper_app_settings)
  connection_strings = [
    {
      name = "CONNECTION_STRING_SHARED_BLOB"
      type = "Custom"
      value = module.stor_esett.primary_connection_string
    },
    {
      name = "CONNECTION_STRING_DATABASE"
      type = "Custom"
      value = local.connection_string_database
    }
  ]
}

locals {
  default_biztalkshipper_app_settings = {
    WEBSITE_LOAD_CERTIFICATES                                             = resource.azurerm_key_vault_certificate.biztalk_certificate.thumbprint
    BLOB_FILES_ERROR_CONTAINER_NAME                                       = local.blob_files_error_container_name
    BLOB_FILES_SENT_CONTAINER_NAME                                        = local.blob_files_sent_container_name
    BLOB_FILES_CONVERTED_CONTAINER_NAME                                   = local.blob_files_converted_container_name
    BLOB_FILES_ACK_CONTAINER_NAME                                         = local.blob_files_ack_container_name
    "RunIntervalSeconds"                                                  = "20"
    "biztalk:Endpoint"                                                    = "/EL_DataHubService/IntegrationService.svc"
    "biztalk:businessTypeConsumption"                                     = "NBS-RECI"
    "biztalk:businessTypeProduction"                                      = "NBS-MEPI"
    "biztalk:businessTypeExchange"                                        = "NBS-MGXI"
    "Logging__LogLevel__Default"                                          = "Information"
    "Logging__LogLevel__Microsoft"                                        = "Warning"
    "Logging__ApplicationInsights__LogLevel__Default"                     = "Information"
    "Logging__ApplicationInsights__LogLevel__Microsoft"                   = "Warning"
    "Logging__ApplicationInsights__LogLevel__Microsoft.Hosting.Lifetime"  = "Information"
  }
}
