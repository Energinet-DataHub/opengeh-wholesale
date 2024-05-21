resource "azurerm_servicebus_topic" "time_series_imported_messages_topic" {
  name                          = "sbt-${lower(var.domain_name_short)}-imported-time-series-messages"
  namespace_id                  =  data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
  max_message_size_in_kilobytes = 102400
}
