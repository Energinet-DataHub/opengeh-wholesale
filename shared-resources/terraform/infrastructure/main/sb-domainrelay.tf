module "sb_domain_relay" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-namespace?ref=v13"

  name                          = "domain-relay"
  project_name                  = var.domain_name_short
  environment_short             = var.environment_short
  environment_instance          = var.environment_instance
  resource_group_name           = azurerm_resource_group.this.name
  location                      = azurerm_resource_group.this.location
  private_endpoint_subnet_id    = data.azurerm_subnet.snet_private_endpoints.id
  ip_restriction_allow_ip_range = local.ip_restrictions_as_string
  auth_rules = [
    {
      name   = "listen",
      listen = true
    },
    {
      name = "send",
      send = true
    },
    {
      name   = "transceiver",
      send   = true
      listen = true
    },
    {
      name   = "manage",
      send   = true
      listen = true
      manage = true
    },
  ]
}

module "kvs_sb_domain_relay_listen_connection_string" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "sb-domain-relay-listen-connection-string"
  value        = module.sb_domain_relay.primary_connection_strings["listen"]
  key_vault_id = module.kv_shared.id
}

module "kvs_sb_domain_relay_send_connection_string" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "sb-domain-relay-send-connection-string"
  value        = module.sb_domain_relay.primary_connection_strings["send"]
  key_vault_id = module.kv_shared.id
}

module "kvs_sb_domain_relay_transceiver_connection_string" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "sb-domain-relay-transceiver-connection-string"
  value        = module.sb_domain_relay.primary_connection_strings["transceiver"]
  key_vault_id = module.kv_shared.id
}

module "kvs_sb_domain_relay_manage_connection_string" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "sb-domain-relay-manage-connection-string"
  value        = module.sb_domain_relay.primary_connection_strings["manage"]
  key_vault_id = module.kv_shared.id
}

module "kvs_sb_domain_relay_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "sb-domain-relay-namespace-id"
  value        = module.sb_domain_relay.id
  key_vault_id = module.kv_shared.id
}

module "kvs_sb_domain_relay_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "sb-domain-relay-namespace-name"
  value        = module.sb_domain_relay.name
  key_vault_id = module.kv_shared.id
}
