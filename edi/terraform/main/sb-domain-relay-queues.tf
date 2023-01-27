# Queue to request change of accounting point characteristics transactions
module "sbq_create_metering_point_transactions" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=v10"

  name                = "create-metering-point-transactions"
  namespace_id        = data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
  project_name        = var.domain_name_short
}

module "sbq_incoming_change_supplier_messagequeue" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=v10"

  name                = "change-supplier-transactions"
  namespace_id        = data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
  project_name        = var.domain_name_short
}

module "sbq_incoming_change_customer_characteristics_message_queue" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=v10"

  name                = "change-customer-characteristics-transactions"
  namespace_id        = data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
  project_name        = var.domain_name_short
}

module "sbq_customermasterdatarequestqueue" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=v10"
  name                = "customermasterdatarequestqueue"
  namespace_id        = data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
  project_name        = var.domain_name_short
}
module "sbq_customermasterdataresponsequeue" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=v10"
  name                = "customermasterdataresponsequeue"
  namespace_id        = data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
  project_name        = var.domain_name_short
}

module "kvs_sbq_create_metering_point_transactions" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "sbq-create-metering-point-transactions"
  value         = module.sbq_create_metering_point_transactions.name
  key_vault_id  = data.azurerm_key_vault.kv_shared_resources.id
}

module "sbq_customermasterdataupdaterequestqueue" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=v10"
  name                = "customermasterdataupdaterequestqueue"
  namespace_id        = data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
  project_name        = var.domain_name_short
}

module "sbq_customermasterdataupdateresponsequeue" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=v10"
  name                = "customermasterdataupdateresponsequeue"
  namespace_id        = data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
  project_name        = var.domain_name_short
}