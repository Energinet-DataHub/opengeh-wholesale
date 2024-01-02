data "azurerm_key_vault" "kv_shared_resources" {
  name                = "kvshres${lower(var.environment_short)}we${lower(var.environment_instance)}"
  resource_group_name = data.azurerm_resource_group.shared.name
}

data "azurerm_key_vault_secret" "primary_action_group_id" {
  name         = "ag-primary-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "appi_instrumentation_key" {
  name         = "appi-shared-instrumentation-key"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "appi_id" {
  name         = "appi-shared-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "appi_name" {
  name         = "appi-shared-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "snet_private_endpoints_id" {
  name         = "snet-private-endpoints-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "log_shared_id" {
  name         = "log-shared-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "st_data_lake_id" {
  name         = "st-data-lake-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "st_data_lake_name" {
  name         = "st-data-lake-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "vnet_resource_group_name" {
  name         = "vnet-resource-group-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

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

data "azurerm_key_vault_secret" "snet_vnet_integration_id" {
  name         = "snet-vnet-integration-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "plan_shared_id" {
  name         = "plan-services-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "plan_shared_name" {
  name         = "plan-services-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "apim_instance_name" {
  name         = "apim-instance-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "apim_instance_resource_group_name" {
  name         = "apim-instance-resource-group-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "apim_timeseriesapi_app_id" {
  name         = "backend-timeseriesapi-app-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "apim_eloverblik_timeseriesapi_client_app_id" {
  name         = "eloverblik-timeseriesapi-client-app-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "apim_b2c_tenant_id" {
  name         = "b2c-tenant-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "apim_logger_id" {
  name         = "apim-logger-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "apim_oauth_server_name" {
  name         = "apim-oauth-server-name"
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

data "azurerm_key_vault_secret" "private_dns_zone_resource_group_name" {
  name         = "private-dns-zone-resource-group-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "st_data_lake_blob_private_ip_address" {
  name         = "st-data-lake-blob-private-ip-address"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "st_data_lake_dfs_private_ip_address" {
  name         = "st-data-lake-dfs-private-ip-address"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
