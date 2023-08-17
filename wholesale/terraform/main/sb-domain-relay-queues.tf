module "sbq_wholesale_inbox_messagequeue" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=v12"

  name         = "wholesale-inbox"
  namespace_id = data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
  project_name = var.domain_name_short
}
