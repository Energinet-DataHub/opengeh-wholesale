module "azfun_enrichmentingest" {
  source                                   = "./modules/azure-azfun-module"
  name                                     = "func-enrichmentingestor-${local.name_suffix}"
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  app_service_plan_id                      = module.plan_services.id
  application_insights_instrumentation_key = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  app_settings = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE     = true,
    WEBSITE_RUN_FROM_PACKAGE            = 1,
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = true,
    # EndRegion
    BLOB_FILES_ENRICHMENTS_CONTAINER_NAME = local.blob_files_enrichments_container.name
  }
  connection_strings = {
    CONNECTION_STRING_SHARED_BLOB = module.stor_esett.primary_connection_string
  }
}
