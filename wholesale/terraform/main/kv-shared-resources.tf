data "azurerm_key_vault" "kv_shared_resources" {
  name                = "kvshres${lower(var.environment_short)}we${lower(var.environment_instance)}"
  resource_group_name = data.azurerm_resource_group.shared.name
}

data "azurerm_key_vault_secret" "snet_vnet_integration_id" {
  name         = "snet-vnet-integration-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "snet_private_endpoints_id" {
  name         = "snet-private-endpoints-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

//
// Application Insights
//

data "azurerm_key_vault_secret" "appi_shared_connection_string" {
  name         = "appi-shared-connection-string"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "appi_shared_id" {
  name         = "appi-shared-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}


//
// Log Analytic Workspace
//

data "azurerm_key_vault_secret" "log_shared_id" {
  name         = "log-shared-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

//
// DataLake
//

data "azurerm_key_vault_secret" "st_data_lake_name" {
  name         = "st-data-lake-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "st_data_lake_id" {
  name         = "st-data-lake-id"
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

//
// Settlement Report storage account
//

data "azurerm_key_vault_secret" "st_settlement_report_name" {
  name         = "st-settlement-report-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "st_settlement_report_id" {
  name         = "st-settlement-report-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "st_settlement_report_blob_private_ip_address" {
  name         = "st-settlement-report-blob-private-ip-address"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "st_settlement_report_dfs_private_ip_address" {
  name         = "st-settlement-report-dfs-private-ip-address"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "settlement_report_external_location_url" {
  name         = "settlement-report-external-location-url"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

//
// ServiceBus
//

data "azurerm_key_vault_secret" "sb_domainrelay_namespace_id" {
  name         = "sb-domain-relay-namespace-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbt_domainrelay_integrationevent_received_id" {
  name         = "sbt-shres-integrationevent-received-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbq_wholesale_inbox_id" {
  name         = "sbq-wholesale-inbox-messagequeue-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbq_edi_inbox_id" {
  name         = "sbq-edi-inbox-messagequeue-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

//
// Storage
//

data "azurerm_key_vault_secret" "st_audit_shres_id" {
  name         = "st-audit-shres-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "st_audit_shres_name" {
  name         = "st-audit-shres-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

//
// Dead-letter logs
//

data "azurerm_key_vault_secret" "st_deadltr_shres_id" {
  name         = "st-deadltr-shres-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
