module "func_biztalkreceiver" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13-without-vnet"

  name                                      = "biztalkreceiver"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  app_service_plan_id                       = module.plan_services.id
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  app_settings = {
    BLOB_FILES_ERROR_CONTAINER_NAME         = local.blob_files_error_container.name
    BLOB_FILES_CONFIRMED_CONTAINER_NAME     = local.blob_files_confirmed_container.name
    BLOB_FILES_SENT_CONTAINER_NAME          = local.blob_files_sent_container.name
    BLOB_FILES_OTHER_CONTAINER_NAME         = local.blob_files_other_container.name
    BLOB_FILES_MGA_IMBALANCE_CONTAINER_NAME = local.blob_files_mga_imbalance_container.name
    BLOB_FILES_BRP_CHANGE_CONTAINER_NAME    = local.blob_files_brp_change_container.name
    CONNECTION_STRING_DATABASE              = "Server=${module.mssql_esett.fully_qualified_domain_name};Database=${module.mssqldb_esett.name};User Id=${local.sqlServerAdminName};Password=${random_password.sqlsrv_admin_password.result};"
  }
  connection_strings = [
    {
      name = "CONNECTION_STRING_SHARED_BLOB"
      type = "Custom"
      value = module.stor_esett.primary_connection_string
    }
  ]
}
