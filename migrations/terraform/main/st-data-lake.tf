module "st_migrations" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v13"

  name                            = "migrations"
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

resource "azurerm_role_assignment" "ra_migrations_contributor" {
  scope                = module.st_migrations.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.id
}

resource "azurerm_storage_container" "bronze" {
  name                  = "bronze"
  storage_account_name  = module.st_migrations.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "silver" {
  name                  = "silver"
  storage_account_name  = module.st_migrations.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "gold" {
  name                  = "gold"
  storage_account_name  = module.st_migrations.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "eloverblik" {
  name                  = "eloverblik"
  storage_account_name  = module.st_migrations.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "wholesale" {
  name                  = "wholesale"
  storage_account_name  = module.st_migrations.name
  container_access_type = "private"
}
