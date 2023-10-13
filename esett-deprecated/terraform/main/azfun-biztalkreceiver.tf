module "azfun_biztalkreceiver" {
  source                                   = "./modules/azure-azfun-module"
  name                                     = "func-biztalkreceiver-${local.name_suffix}"
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
    BLOB_FILES_ERROR_CONTAINER_NAME         = local.blob_files_error_container.name
    BLOB_FILES_CONFIRMED_CONTAINER_NAME     = local.blob_files_confirmed_container.name
    BLOB_FILES_SENT_CONTAINER_NAME          = local.blob_files_sent_container.name
    BLOB_FILES_OTHER_CONTAINER_NAME         = local.blob_files_other_container.name
    BLOB_FILES_MGA_IMBALANCE_CONTAINER_NAME = local.blob_files_mga_imbalance_container.name
    BLOB_FILES_BRP_CHANGE_CONTAINER_NAME    = local.blob_files_brp_change_container.name
    CONNECTION_STRING_DATABASE              = "Server=${module.sqlsrv_esett.fully_qualified_domain_name};Database=${module.sqldb_esett.name};User Id=${local.sqlServerAdminName};Password=${random_password.sqlsrv_admin_password.result};"
  }
  connection_strings = {
    CONNECTION_STRING_SHARED_BLOB = module.stor_esett.primary_connection_string
  }
}
