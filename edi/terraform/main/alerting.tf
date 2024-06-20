module "monitor_action_group_edi" {
  count  = var.alert_email_address != null ? 1 : 0
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=14.22.0"

  name                 = "alerts"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name                      = "edi-mon-grp"
  email_receiver_name             = "Alerts-Edi-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  email_receiver_address          = var.alert_email_address
  custom_dimension_subsystem_name = "edi"

  query_alerts_list = [
    {
      name        = "exception-trigger"
      description = "Alert when total results cross threshold"
      query       = <<-QUERY
        exceptions
        | where timestamp > ago(10m)
            and (cloud_RoleName == 'func-api-${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}'
            or cloud_RoleName == 'app-b2cwebapi-${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}')
          and (type !has "Energinet.DataHub.EDI" and type !hasprefix "NotSupported")
        QUERY
    },
  ]
  application_insights_id = data.azurerm_key_vault_secret.appi_shared_id.value
}
