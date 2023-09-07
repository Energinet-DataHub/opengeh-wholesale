resource "azurerm_monitor_action_group" "edi" {
  name                = "ag-edi-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name = azurerm_resource_group.this.name
  short_name          = "ag-ed-${lower(var.environment_short)}-${lower(var.environment_instance)}"

  email_receiver {
    name                    = "Alerts-Edi-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
    email_address           = "046adedd.energinet.onmicrosoft.com@emea.teams.ms"
    use_common_alert_schema = true
  }
}


resource "azurerm_monitor_scheduled_query_rules_alert" "edi_alert" {
  name                = "alert-edi-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  location            = azurerm_resource_group.this.location
  resource_group_name = var.shared_resources_resource_group_name

  action {
    action_group = [azurerm_monitor_action_group.edi.id]
  }
  data_source_id = data.azurerm_key_vault_secret.appi_shared_id.value
  description    = "Alert when total results cross threshold"
  enabled        = true
  query          = <<-QUERY
                  requests
              | where timestamp > ago(10m) and  success == false
              | join kind= inner (
              exceptions
              | where timestamp > ago(10m)
                and (cloud_RoleName == 'func-api-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}')
              ) on operation_Id
              | project exceptionType = type, failedMethod = method, requestName = name, requestDuration = duration, function = cloud_RoleName
                QUERY
  severity       = 1
  frequency      = 5
  time_window    = 10
  trigger {
    operator  = "GreaterThan"
    threshold = 0
  }
}
