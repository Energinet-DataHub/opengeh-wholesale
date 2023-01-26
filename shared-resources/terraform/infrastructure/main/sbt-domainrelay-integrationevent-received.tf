module "sbt_domainrelay_integrationevent_received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=v10"

  name                = "integrationevent-received"
  namespace_id        = module.sb_domain_relay.id
  project_name        = var.domain_name_short
}

module "kvs_sbt_domainrelay_integrationevent_received_id" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "${module.sbt_domainrelay_integrationevent_received.name}-id" # name will be sbt-sharedres-integrationevent-received-id due to naming convention enforced by the TF module
  value         = module.sbt_domainrelay_integrationevent_received.id
  key_vault_id  = module.kv_shared.id
}

module "kvs_sbt_domainrelay_integrationevent_received_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "${module.sbt_domainrelay_integrationevent_received.name}-name" # name will be sbt-sharedres-integrationevent-received-name due to naming convention enforced by the TF module
  value         = module.sbt_domainrelay_integrationevent_received.name
  key_vault_id  = module.kv_shared.id
}
