module "log_workspace_shared" {
  source               = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/log-workspace?ref=v13"
  name                 = "shared"
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  sku                  = "PerGB2018"
  retention_in_days    = var.log_retention_in_days
  project_name         = var.domain_name_short
}

module "kvs_log_shared_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "log-shared-name"
  value        = module.log_workspace_shared.name
  key_vault_id = module.kv_shared.id
}

module "kvs_log_shared_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "log-shared-id"
  value        = module.log_workspace_shared.id
  key_vault_id = module.kv_shared.id
}

module "kvs_log_shared_workspace_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "log-shared-workspace-id"
  value        = module.log_workspace_shared.workspace_id
  key_vault_id = module.kv_shared.id
}
