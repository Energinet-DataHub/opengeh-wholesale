module "ag_primary" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=13.32.0"

  name                 = "primary"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name             = "Primary"
  email_receiver_name    = "DevOps"
  email_receiver_address = var.ag_primary_email_address
}

module "kvs_ag_primary_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "ag-primary-id"
  value        = module.ag_primary.id
  key_vault_id = module.kv_shared.id
}

module "kvs_ag_primary_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "ag-primary-name"
  value        = module.ag_primary.name
  key_vault_id = module.kv_shared.id
}
