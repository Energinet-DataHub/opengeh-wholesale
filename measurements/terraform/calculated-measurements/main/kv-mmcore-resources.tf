data "azurerm_key_vault" "kv_mmcore" {
  name                = "kvmmcore${lower(var.environment_short)}we${lower(var.environment_instance)}"
  resource_group_name = data.azurerm_resource_group.measurements_core.name
}

data "azurerm_key_vault_secret" "mmcore_databricks_workspace_url" {
  name         = "dbw-workspace-url"
  key_vault_id = data.azurerm_key_vault.kv_mmcore.id
}

