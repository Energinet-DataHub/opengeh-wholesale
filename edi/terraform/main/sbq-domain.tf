resource "azurerm_servicebus_queue" "edi_incoming_messages_queue" {
  name                         = "sbq-${lower(var.domain_name_short)}-incoming-messages"
  namespace_id                 =  data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
}

