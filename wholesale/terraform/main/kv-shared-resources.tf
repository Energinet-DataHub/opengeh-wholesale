data "azurerm_key_vault" "kv_shared_resources" {
  name                = var.shared_resources_keyvault_name
  resource_group_name = var.shared_resources_resource_group_name
}

data "azurerm_key_vault_secret" "primary_action_group_id" {
  name         = "ag-primary-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "snet_vnet_integration_id" {
  name         = "snet-vnet-integration-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "snet_private_endpoints_id" {
  name         = "snet-private-endpoints-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "plan_shared_id" {
  name         = "plan-services-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "appi_shared_instrumentation_key" {
  name         = "appi-shared-instrumentation-key"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "log_shared_id" {
  name         = "log-shared-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "st_shared_data_lake_name" {
  name         = "st-data-lake-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "st_shared_data_lake_id" {
  name         = "st-data-lake-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

# ID of the shared servicebus namespace
data "azurerm_key_vault_secret" "sb_integration_events_id" {
  name         = "sb-domain-relay-namespace-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

# Deployment agents subnet id
data "azurerm_key_vault_secret" "snet_vnet_deployment_agents_id" {
  name         = "snet-deployment-agents-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
