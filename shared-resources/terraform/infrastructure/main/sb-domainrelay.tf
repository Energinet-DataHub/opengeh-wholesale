module "sb_domain_relay" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-namespace?ref=service-bus-namespace_8.1.0"

  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  private_endpoint_subnet_id = azurerm_subnet.privateendpoints.id
  ip_restrictions            = var.ip_restrictions
  audit_storage_account = var.enable_audit_logs ? {
    id = module.st_audit_logs.id
  } : null
}

resource "azurerm_role_assignment" "spn_sbns" {
  scope                = module.sb_domain_relay.id
  role_definition_name = "Azure Service Bus Data Owner"
  principal_id         = data.azurerm_client_config.current.object_id
}

module "kvs_sb_domain_relay_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sb-domain-relay-namespace-id"
  value        = module.sb_domain_relay.id
  key_vault_id = module.kv_shared.id
}

module "kvs_sb_domain_relay_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sb-domain-relay-namespace-name"
  value        = module.sb_domain_relay.name
  key_vault_id = module.kv_shared.id
}

module "kvs_sb_domain_relay_endpoint" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sb-domain-relay-namespace-endpoint"
  value        = module.sb_domain_relay.endpoint
  key_vault_id = module.kv_shared.id
}
