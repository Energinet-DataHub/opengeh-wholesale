module "app_biztalkshipper" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v13-without-vnet"

  name                                      = "biztalkshipper"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  app_service_plan_id                       = module.plan_services.id
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  connection_strings = [
    {
      name = "CONNECTION_STRING_SHARED_BLOB"
      type = "Custom"
      value = module.stor_esett.primary_connection_string
    }
  ]
}

locals {
  default_biztalkshipper_app_settings = {
    WEBSITE_LOAD_CERTIFICATES           = resource.azurerm_key_vault_certificate.biztalk_certificate.thumbprint
    BLOB_FILES_ERROR_CONTAINER_NAME     = local.blob_files_error_container.name
    BLOB_FILES_SENT_CONTAINER_NAME      = local.blob_files_sent_container.name
    BLOB_FILES_CONVERTED_CONTAINER_NAME = local.blob_files_converted_container.name
    BLOB_FILES_ACK_CONTAINER_NAME       = local.blob_files_ack_container.name
    "biztalk:Endpoint"                  = "/EL_DataHubService/IntegrationService.svc"
    "biztalk:businessTypeConsumption"   = "NBS-RECI"
    "biztalk:businessTypeProduction"    = "NBS-MEPI"
    "biztalk:businessTypeExchange"      = "NBS-MGXI"
  }
}
