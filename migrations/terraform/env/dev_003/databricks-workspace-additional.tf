# Temporary file until a separate Databricks workspace is available for the Electricity market
#
# Places Databricks secrets in shared key vault so other subsystems can use REST API or data.
#

module "kvs_mig_dbw_workspace_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "dbw-migration-workspace-url"
  value        = module.dbw.workspace_url
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

module "kvs_mig_dbw_workspace_token" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "dbw-migration-workspace-token"
  value        = module.dbw.databricks_token
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
