module "app_importer" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=v13"

  name                                      = "importer"
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
  use_dotnet_isolated_runtime               = true
  dotnet_framework_version                  = "v7.0"
  app_settings = {
    # EndRegion
    WEBSITE_LOAD_CERTIFICATES       = resource.azurerm_key_vault_certificate.dh2_certificate.thumbprint
    BLOB_FILES_ERROR_CONTAINER_NAME = local.blob_files_error_container.name
    BLOB_FILES_RAW_CONTAINER_NAME   = local.blob_files_raw_container.name
    RunIntervalSeconds              = "20"
    Endpoint                        = "https://b2b.te7.datahub.dk"
    NamespacePrefix                 = "ns0"
    NamespaceUri                    = "un:unece:260:data:EEM-DK_AggregatedMeteredDataTimeSeriesForNBS:v3"
    ResponseNamespaceUri            = "urn:www:datahub:dk:b2b:v01"
    CONNECTION_STRING_DATABASE      = "Server=${module.mssql_esett.fully_qualified_domain_name};Database=${module.mssqldb_esett.name};User Id=${local.sqlServerAdminName};Password=${random_password.sqlsrv_admin_password.result};"
  }
  connection_strings = [
    {
      name = "CONNECTION_STRING_SHARED_BLOB"
      type = "Custom"
      value = module.stor_esett.primary_connection_string
    }
  ]
}

resource "azurerm_role_assignment" "importer_developer_access" {
  for_each = toset(var.developer_object_ids)

  scope                = module.app_importer.id
  role_definition_name = "Contributor"
  principal_id         = each.value
}

