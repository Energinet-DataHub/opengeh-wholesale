module "log_workspace_shared" {
  source               = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/log-workspace?ref=log-workspace_7.0.0"
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  sku                  = "PerGB2018"
  retention_in_days    = var.log_retention_in_days
  project_name         = var.domain_name_short
}

module "kvs_log_shared_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "log-shared-name"
  value        = module.log_workspace_shared.name
  key_vault_id = module.kv_shared.id
}

module "kvs_log_shared_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "log-shared-id"
  value        = module.log_workspace_shared.id
  key_vault_id = module.kv_shared.id
}

module "kvs_log_shared_workspace_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "log-shared-workspace-id"
  value        = module.log_workspace_shared.workspace_id
  key_vault_id = module.kv_shared.id
}

# Tables - only adjusted on preprod and prod
resource "azurerm_log_analytics_workspace_table" "alert" {
  count = var.environment_short == "p" || var.environment_short == "b" ? 1 : 0

  workspace_id            = module.log_workspace_shared.id
  name                    = "Alert"
  retention_in_days       = 365
  total_retention_in_days = 365
}

resource "azurerm_log_analytics_workspace_table" "azure_activity" {
  count = var.environment_short == "p" || var.environment_short == "b" ? 1 : 0

  workspace_id            = module.log_workspace_shared.id
  name                    = "AzureActivity"
  retention_in_days       = 183
  total_retention_in_days = 183
}

# Only exists on prod, as diagnostics is only enabled on prod
resource "azurerm_log_analytics_workspace_table" "azure_diagnostics" {
  count = var.environment_short == "p" ? 1 : 0

  workspace_id            = module.log_workspace_shared.id
  name                    = "AzureDiagnostics"
  retention_in_days       = 183
  total_retention_in_days = 183
}

resource "azurerm_log_analytics_workspace_table" "azure_metrics" {
  count = var.environment_short == "p" || var.environment_short == "b" ? 1 : 0

  workspace_id            = module.log_workspace_shared.id
  name                    = "AzureMetrics"
  retention_in_days       = 183
  total_retention_in_days = 183
}
