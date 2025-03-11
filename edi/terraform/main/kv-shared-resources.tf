data "azurerm_key_vault" "kv_shared_resources" {
  name                = "kvshres${lower(var.environment_short)}we${lower(var.environment_instance)}"
  resource_group_name = data.azurerm_resource_group.shared.name
}

data "azurerm_key_vault_secret" "mssql_data_name" {
  name         = "mssql-data-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "mssql_data_resource_group_name" {
  name         = "mssql-data-resource-group-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "mssql_data_url" {
  name         = "mssql-data-url"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "appi_shared_id" {
  name         = "appi-shared-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "log_shared_id" {
  name         = "log-shared-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "snet_privateendpoints_id" {
  name         = "snet-privateendpoints-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "snet_vnetintegrations_id" {
  name         = "snet-vnetintegrations-id"
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

data "azurerm_key_vault_secret" "apim_b2b_app_id" {
  name         = "backend-b2b-app-id"
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

data "azurerm_key_vault_secret" "api_backend_open_id_url" {
  name         = "api-backend-open-id-url"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "backend_bff_app_id" {
  name         = "backend-bff-app-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "appi_shared_connection_string" {
  name         = "appi-shared-connection-string"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

//
// ServiceBus
//

data "azurerm_key_vault_secret" "sb_domain_relay_namespace_id" {
  name         = "sb-domain-relay-namespace-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbt_domainrelay_integrationevent_received_id" {
  name         = "sbt-shres-integrationevent-received-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbt_processmanagerstart_id" {
  name         = "sbt-processmanagerstart-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbt_processmanagernotify_id" {
  name         = "sbt-processmanagernotify-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbt_brs021forwardmetereddatastart_id" {
  name         = "sbt-brs021forwardmetereddatastart-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbt_brs021forwardmetereddatanotify_id" {
  name         = "sbt-brs021forwardmetereddatanotify-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbt_edi_id" {
  name         = "sbt-edi-id"
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

data "azurerm_key_vault_secret" "apim_instance_id" {
  name         = "apim-instance-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "st_audit_shres_name" {
  name         = "st-audit-shres-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "st_audit_shres_id" {
  name         = "st-audit-shres-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

//
// Dead-letter logs
//

data "azurerm_key_vault_secret" "st_deadltr_shres_id" {
  name         = "st-deadltr-shres-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

// Backup vault
data "azurerm_key_vault_secret" "bvault_vaulted_policy_id" {
  name         = "bvault-vaulted-policy-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "bvault_vault_id" {
  name         = "bvault-vault-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "bvault_vault_location" {
  name         = "bvault-vault-location"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "bvault_vault_principal_id" {
  name         = "bvault-vault-principal-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
