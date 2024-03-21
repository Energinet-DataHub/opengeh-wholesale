resource "azurerm_servicebus_queue" "time_series_imported_messages_queue" {
  name                         = "sbq-${lower(var.domain_name_short)}-imported-time-series-messages"
  namespace_id                 =  data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
}
