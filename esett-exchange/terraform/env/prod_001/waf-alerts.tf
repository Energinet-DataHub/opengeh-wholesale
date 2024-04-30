resource "azurerm_monitor_scheduled_query_rules_alert" "waf_alert" {
  name                = "qra-waf-${local.NAME_SUFFIX}"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name

  action {
    action_group = [module.ag_dev_alert.id]
  }
  data_source_id = data.azurerm_key_vault_secret.log_shared_id.value
  description    = "Blocked requests from IP 194.239.2.103 or 194.239.2.105"
  enabled        = true
  query          = <<-QUERY
    AzureDiagnostics
    | where ResourceProvider == "MICROSOFT.CDN" and Category == "FrontDoorWebApplicationFirewallLog"
    | where action_s == "Block"
    | where clientIP_s == "194.239.2.105" or clientIP_s == "194.239.2.103"
  QUERY
  severity       = 1
  frequency      = 5
  time_window    = 5
  trigger {
    operator  = "GreaterThan"
    threshold = 0
  }
}
