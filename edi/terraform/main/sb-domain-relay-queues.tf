module "sbq_incoming_change_supplier_messagequeue" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=v10"

  name                = "change-supplier-transactions"
  namespace_id        = data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
  project_name        = var.domain_name_short
}