data "azurerm_key_vault_secret" "dbw_databricks_workspace_url" {
  name         = "dbw-shared-workspace-url"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "dbw_databricks_workspace_id" {
  name         = "dbw-shared-workspace-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "dbw_databricks_workspace_token" {
  name         = "dbw-shared-workspace-token"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "main_virtual_network_id" {
  name         = "vnet-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "main_virtual_network_name" {
  name         = "vnet-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "main_virtual_network_resource_group_name" {
  name         = "vnet-resource-group-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
