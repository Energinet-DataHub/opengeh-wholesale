data "azurerm_key_vault" "kv_pmcore" {
  name                = "kvpmcore${lower(var.environment_short)}we${lower(var.environment_instance)}"
  resource_group_name = data.azurerm_resource_group.process_manager_core.name
}

data "azurerm_key_vault_secret" "func_service_plan_id" {
  name         = "func-service-plan-id"
  key_vault_id = data.azurerm_key_vault.kv_pmcore.id
}

data "azurerm_key_vault_secret" "mssqldb_server_name" {
  name         = "mssql-pm-database-server-name"
  key_vault_id = data.azurerm_key_vault.kv_pmcore.id
}

data "azurerm_key_vault_secret" "mssqldb_name" {
  name         = "mssql-pm-database-name"
  key_vault_id = data.azurerm_key_vault.kv_pmcore.id
}

data "azurerm_key_vault_secret" "mssqldb_connection_string" {
  name         = "mssqldb-connection-string"
  key_vault_id = data.azurerm_key_vault.kv_pmcore.id
}

data "azurerm_key_vault_secret" "st_taskhub_primary_connection_string" {
  name         = "st-taskhub-primary-connection-string"
  key_vault_id = data.azurerm_key_vault.kv_pmcore.id
}

data "azurerm_key_vault_secret" "st_taskhub_id" {
  name         = "st-taskhub-id"
  key_vault_id = data.azurerm_key_vault.kv_pmcore.id
}

data "azurerm_key_vault_secret" "st_taskhub_hub_name" {
  name         = "st-taskhub-hub-name"
  key_vault_id = data.azurerm_key_vault.kv_pmcore.id
}

data "azurerm_key_vault_secret" "monitor_action_group_id" {
  count = var.monitor_action_group_exists == false ? 0 : 1

  name         = "monitor-action-group-id"
  key_vault_id = data.azurerm_key_vault.kv_pmcore.id
}
