# Temporary file until a separate Databricks workspace is available for the Electricity market

module "kvs_mig_dbw_warehouse_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "dbw-migration-warehouse-id"
  value        = resource.databricks_sql_endpoint.migration_sql_endpoint.id
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
