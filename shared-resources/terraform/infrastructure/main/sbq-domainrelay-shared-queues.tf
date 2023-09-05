module "sbq_wholesale_inbox_messagequeue" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=v12"

  name         = "wholesale-inbox"
  namespace_id = module.sb_domain_relay.id
  project_name = var.domain_name_short
}

module "kvs_sbq_wholesale_inbox_messagequeue_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "sbq-wholesale-inbox-messagequeue-name"
  value        = module.sbq_wholesale_inbox_messagequeue.name
  key_vault_id = module.kv_shared.id
}

module "sbq_edi_inbox_messagequeue" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=v12"

  name         = "edi-inbox"
  namespace_id = module.sb_domain_relay.id
  project_name = var.domain_name_short
}

module "kvs_sbq_edi_inbox_messagequeue" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "sbq-edi-inbox-messagequeue-name"
  value        = module.sbq_edi_inbox_messagequeue.name
  key_vault_id = module.kv_shared.id
}

