module "func_biztalkreceiver" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=v13"

  name                                      = "biztalkreceiver"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  vnet_integration_subnet_id                = data.azurerm_key_vault_secret.snet_vnet_integration_id.value
  private_endpoint_subnet_id                = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                       = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  dotnet_framework_version                  = "v6.0"
  app_settings = {
    BLOB_FILES_ERROR_CONTAINER_NAME         = local.blob_files_error_container.name
    BLOB_FILES_CONFIRMED_CONTAINER_NAME     = local.blob_files_confirmed_container.name
    BLOB_FILES_SENT_CONTAINER_NAME          = local.blob_files_sent_container.name
    BLOB_FILES_OTHER_CONTAINER_NAME         = local.blob_files_other_container.name
    BLOB_FILES_MGA_IMBALANCE_CONTAINER_NAME = local.blob_files_mga_imbalance_container.name
    BLOB_FILES_BRP_CHANGE_CONTAINER_NAME    = local.blob_files_brp_change_container.name
    CONNECTION_STRING_DATABASE              = local.connection_string_database
  }
  connection_strings = [
    {
      name = "CONNECTION_STRING_SHARED_BLOB"
      type = "Custom"
      value = module.stor_esett.primary_connection_string
    }
  ]
}
