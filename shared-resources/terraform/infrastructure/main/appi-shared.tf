module "appi_shared" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/application-insights?ref=v13"

  name                       = "shared"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  log_analytics_workspace_id = module.log_workspace_shared.id
  storage_account_sourcemaps = module.st_source_maps.name
}

module "kvs_appi_shared_connection_string" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "appi-shared-connection-string"
  value        = module.appi_shared.connection_string
  key_vault_id = module.kv_shared.id
}

module "kvs_appi_shared_instrumentation_key" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "appi-shared-instrumentation-key"
  value        = module.appi_shared.instrumentation_key
  key_vault_id = module.kv_shared.id
}

module "kvs_appi_shared_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "appi-shared-name"
  value        = module.appi_shared.name
  key_vault_id = module.kv_shared.id
}

module "kvs_appi_shared_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "appi-shared-id"
  value        = module.appi_shared.id
  key_vault_id = module.kv_shared.id
}
