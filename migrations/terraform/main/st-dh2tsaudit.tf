module "st_dh2timeseries_audit" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v14"

  name                       = "dh2tsaudit"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "LRS"
  access_tier                = "Cool"
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                   = local.ip_restrictions_as_string
  prevent_deletion           = true
}

#---- Role assignments

resource "azurerm_role_assignment" "ra_dh2timeseriesaudit_contributor" {
  scope                = module.st_dh2timeseries_audit.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.id
}

#---- Containers

resource "azurerm_storage_container" "timeseriesaudit" {
  name                  = "dh2-time-series-audit"
  storage_account_name  = module.st_dh2timeseries_audit.name
  container_access_type = "private"
}
