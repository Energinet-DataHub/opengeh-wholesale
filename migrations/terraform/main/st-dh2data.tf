module "st_dh2data" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v13"

  name                            = "dh2data"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  account_replication_type        = "LRS"
  account_tier                    = "Standard"
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  private_dns_resource_group_name = data.azurerm_resource_group.shared.name
  ip_rules                        = var.hosted_deployagent_public_ip_range
  prevent_deletion                = true
}

#---- Role assignments

resource "azurerm_role_assignment" "ra_dh2data_contributor" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.id
}

#---- Containers

resource "azurerm_storage_container" "dh2_metering_point_history" {
  name                  = "dh2-metering-point-history"
  storage_account_name  = module.st_dh2data.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "dh2_timeseries" {
  name                  = "dh2-timeseries"
  storage_account_name  = module.st_dh2data.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "dh2_timeseries_synchronization" {
  name                  = "dh2-timeseries-synchronization"
  storage_account_name  = module.st_dh2data.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "dh2_charges" {
  name                  = "dh2-charges"
  storage_account_name  = module.st_dh2data.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "dh2_charge_links" {
  name                  = "dh2-charge-links"
  storage_account_name  = module.st_dh2data.name
  container_access_type = "private"
}
