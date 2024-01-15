resource "azurerm_servicebus_queue" "wholesale_inbox_messagequeue" {
  name         = "sbq-${lower(var.domain_name_short)}-wholesale-inbox"
  namespace_id = module.sb_domain_relay.id
}

module "kvs_sbq_wholesale_inbox_messagequeue_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "sbq-wholesale-inbox-messagequeue-name"
  value        = azurerm_servicebus_queue.wholesale_inbox_messagequeue.name
  key_vault_id = module.kv_shared.id
}

resource "azurerm_servicebus_queue" "edi_inbox_messagequeue" {
  name                          = "sbq-${lower(var.domain_name_short)}-edi-inbox"
  namespace_id                  = module.sb_domain_relay.id
  max_message_size_in_kilobytes = 102400
}

module "kvs_sbq_edi_inbox_messagequeue" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "sbq-edi-inbox-messagequeue-name"
  value        = azurerm_servicebus_queue.edi_inbox_messagequeue.name
  key_vault_id = module.kv_shared.id
}
