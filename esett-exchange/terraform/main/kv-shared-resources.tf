data "azurerm_key_vault" "kv_shared_resources" {
  name                = "kvshres${lower(var.environment_short)}we${lower(var.environment_instance)}"
  resource_group_name = data.azurerm_resource_group.shared.name
}

data "azurerm_key_vault_secret" "appi_shared_connection_string" {
  name         = "appi-shared-connection-string"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "appi_shared_id" {
  name         = "appi-shared-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
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

data "azurerm_key_vault_secret" "sbt_domainrelay_integrationevent_received_id" {
  name         = "sbt-shres-integrationevent-received-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "apim_instance_resource_group_name" {
  name         = "apim-instance-resource-group-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "apim_instance_name" {
  name         = "apim-instance-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "apim_oauth_server_name" {
  name         = "apim-oauth-server-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "apim_logger_id" {
  name         = "apim-logger-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "hc_biztalk_id" {
  name         = "hc-biztalk-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "hc_biztalk_name" {
  name         = "hc-biztalk-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "relay_name" {
  name         = "relay-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

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

data "azurerm_key_vault_secret" "st_audit_shres_name" {
  name         = "st-audit-shres-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "st_audit_shres_id" {
  name         = "st-audit-shres-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
