module "monitor_action_group_mkpt" {
  count  = var.alert_email_address != null ? 1 : 0
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=monitor-action-group-email_5.0.0"

  name                 = "alerts"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name                 = "mkpt-alerts"
  email_receiver_name        = "Alerts-mkpt-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  email_receiver_address     = var.alert_email_address
  custom_dimension_subsystem = ["mark-part"]

  query_alerts_list = [
    {
      name        = "healthcheck-trigger"
      description = "Alert on healthcheck failure"
      query       = <<-QUERY
                      exceptions
                      | where timestamp > ago(10m)
                        and (cloud_RoleName == 'func-certificatesynchronization-${local.NAME_SUFFIX}'
                          or cloud_RoleName == 'func-organization-${local.NAME_SUFFIX}'
                          or cloud_RoleName == 'app-webapi-${local.NAME_SUFFIX}')
                        and (operation_Name == "GET /monitor/ready")
                    QUERY
      severity    = 1
      frequency   = 5
      time_window = 5
      threshold   = 0
      operator    = "GreaterThan"
    }
  ]

  application_insights_id = data.azurerm_key_vault_secret.appi_shared_id.value
}
