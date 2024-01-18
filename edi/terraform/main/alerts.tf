resource "azurerm_monitor_action_group" "this" {
  count               = 1
  name                = "ag-edi-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name = azurerm_resource_group.this.name
  short_name          = "ag-edi"

  email_receiver {
    name                    = "Alerts-Edi-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
    email_address           = "7bc69f85.energinet.onmicrosoft.com@emea.teams.ms"
    use_common_alert_schema = true
  }
}

resource "azurerm_monitor_scheduled_query_rules_alert" "this" {
  count               = 1
  name                = "alert-edi-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  location            = azurerm_resource_group.this.location
  resource_group_name = data.azurerm_resource_group.shared.name

  action {
    action_group = [azurerm_monitor_action_group.this[0].id]
  }
  data_source_id = data.azurerm_key_vault_secret.appi_shared_id.value
  description    = "Alert when total results cross threshold"
  enabled        = true
  query          = <<-QUERY
                  exceptions
                  | where timestamp > ago(10m)
                    and (cloud_RoleName == 'func-api-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}'
                      or cloud_RoleName == 'func-api-${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}'
                      or cloud_RoleName == 'app-b2cwebapi-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}'
                      or cloud_RoleName == 'app-b2cwebapi-${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}')
                    and (type !has "Energinet.DataHub.EDI" and type !hasprefix "NotSupported")
                QUERY
  severity       = 1
  frequency      = 5
  time_window    = 5
  trigger {
    operator  = "GreaterThan"
    threshold = 0
  }
}
