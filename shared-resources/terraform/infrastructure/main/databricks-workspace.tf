module "dbw_shared" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-workspace?ref=v13"

  name                                     = "dbw"
  project_name                             = var.domain_name_short
  environment_short                        = var.environment_short
  environment_instance                     = var.environment_instance
  resource_group_name                      = azurerm_resource_group.this.name
  location                                 = azurerm_resource_group.this.location
  sku                                      = "premium"
  main_virtual_network_id                  = data.azurerm_virtual_network.this.id
  main_virtual_network_name                = data.azurerm_virtual_network.this.name
  main_virtual_network_resource_group_name = data.azurerm_virtual_network.this.resource_group_name
  databricks_virtual_network_address_space = var.databricks_vnet_address_space
  private_subnet_address_prefix            = var.databricks_private_subnet_address_prefix
  public_subnet_address_prefix             = var.databricks_public_subnet_address_prefix
  user_access_security_group_object_id     = var.developers_security_group_object_id
  public_network_service_endpoints = [
    "Microsoft.EventHub"
  ]
}

module "kvs_databricks_workspace_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "dbw-shared-workspace-id"
  value        = module.dbw_shared.id
  key_vault_id = module.kv_shared.id
}

module "kvs_databricks_workspace_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "dbw-shared-workspace-url"
  value        = module.dbw_shared.workspace_url
  key_vault_id = module.kv_shared.id
}

module "kvs_databricks_public_network_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "dbw-public-network-id"
  value        = module.dbw_shared.public_network_id
  key_vault_id = module.kv_shared.id
}

module "kvs_databricks_private_dns_resource_group_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "databricks-private-dns-resource-group-name"
  value        = module.dbw_shared.private_dns_zone_resource_group_name
  key_vault_id = module.kv_shared.id
}

module "kvs_databricks_dbw_shared_workspace_token" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "dbw-shared-workspace-token"
  value        = module.dbw_shared.databricks_token
  key_vault_id = module.kv_shared.id
}

resource "databricks_git_credential" "ado" {
  git_username          = var.github_username
  git_provider          = "gitHub"
  personal_access_token = var.github_personal_access_token
  depends_on            = [module.dbw_shared]
}
