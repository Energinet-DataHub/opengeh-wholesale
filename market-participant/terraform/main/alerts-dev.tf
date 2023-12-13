module "ag_dev_alert" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=v13"

  name                 = "dev"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name             = "ag-dev-mkpt"
  email_receiver_name    = "alerts-dev-${local.NAME_SUFFIX}"
  email_receiver_address = "a9b4c74d.energinet.onmicrosoft.com@emea.teams.ms"

  query_alerts = {
    query          = <<-QUERY
                  exceptions
                  | where timestamp > ago(10m)
                    and (cloud_RoleName == 'func-certificatesynchronization-${local.NAME_SUFFIX}'
                      or cloud_RoleName == 'func-organization-${local.NAME_SUFFIX}'
                      or cloud_RoleName == 'app-webapi-${local.NAME_SUFFIX}')
                    and (operation_Name == "GET /monitor/ready")
                  QUERY
    appinsights_id = data.azurerm_key_vault_secret.appi_shared_id.value
  }
}
