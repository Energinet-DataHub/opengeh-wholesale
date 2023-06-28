module "st_dh2data" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v11"

  name                            = "dh2data"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  account_replication_type        = "LRS"
  account_tier                    = "Standard"
  is_hns_enabled                  = true
  log_analytics_workspace_id      = data.azurerm_key_vault_secret.log_shared_id.value
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  private_dns_resource_group_name = var.shared_resources_resource_group_name
  ip_rules                        = data.azurerm_key_vault_secret.pir_hosted_deployment_agents.value
  containers = [
    {
      name = "dh2-metering-point-history"
    },
    {
      name = "dh2-timeseries"
    },
    {
      name = "dh2-timeseries-synchronization"
    }
  ]
}

resource "azurerm_role_assignment" "ra_dh2data_contributor" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.id
}
