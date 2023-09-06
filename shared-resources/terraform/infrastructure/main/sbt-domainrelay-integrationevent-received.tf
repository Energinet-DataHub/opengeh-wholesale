module "sbt_domainrelay_integrationevent_received" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=v12"

  name         = "integrationevent-received"
  namespace_id = module.sb_domain_relay.id
  project_name = var.domain_name_short
}

module "kvs_sbt_domainrelay_integrationevent_received_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "sbt-shres-integrationevent-received-id"
  value        = module.sbt_domainrelay_integrationevent_received.id
  key_vault_id = module.kv_shared.id
}

module "kvs_sbt_domainrelay_integrationevent_received_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "sbt-shres-integrationevent-received-name"
  value        = module.sbt_domainrelay_integrationevent_received.name
  key_vault_id = module.kv_shared.id
}
