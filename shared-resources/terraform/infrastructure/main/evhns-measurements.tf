module "evhns_measurements" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/eventhub-namespace?ref=eventhub-namespace_7.2.0"

  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  private_endpoint_subnet_id = azurerm_subnet.privateendpoints.id
  network_ruleset = {
    allowed_subnet_ids = [
      azurerm_subnet.vnetintegrations.id
    ]
  }
}

module "kvs_evhns_measurements_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "evhns_measurements-id"
  value        = module.evhns_measurements.id
  key_vault_id = module.kv_shared.id
}

module "kvs_evhns_measurements_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "evhns_measurements-name"
  value        = module.evhns_measurements.name
  key_vault_id = module.kv_shared.id
}
