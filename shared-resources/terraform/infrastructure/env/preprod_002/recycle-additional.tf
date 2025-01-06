# Only used for cycling of shared
module "shared_databricks_workspace_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "databricks-workspace-shared-url"
  value        = azurerm_databricks_workspace.this.workspace_url
  key_vault_id = module.kv_shared.id
}

module "grafana_dashboard_endpoint" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "grafana-dashboard-endpoint"
  value        = azurerm_dashboard_grafana.this.endpoint
  key_vault_id = module.kv_shared.id
}

module "grafana_key" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "grafana-key"
  value        = shell_script.create_key.output["key"]
  key_vault_id = module.kv_shared.id
}
