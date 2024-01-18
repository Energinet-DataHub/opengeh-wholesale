# Give developers dataplane access to test_001

resource "azurerm_role_assignment" "developers_storage_blob_read_access" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = var.developers_security_group_object_id
}

resource "azurerm_role_assignment" "developers_storage_queue_read_access" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Storage Queue Data Reader"
  principal_id         = var.developers_security_group_object_id
}

// Allow developers to access shared keyvault on test_001
resource "azurerm_key_vault_access_policy" "developer_shared_keyvault_read_access" {
  key_vault_id = module.kv_shared.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = var.developers_security_group_object_id

  key_permissions = [
    "Get",
    "List"
  ]

  secret_permissions = [
    "Get",
    "List"
  ]

  certificate_permissions = [
    "Get",
    "List"
  ]
}

# Allow developers to read config settings in Function apps and App Services on test_001
resource "azurerm_role_assignment" "app_config_settings_read_access" {
  count                = 1
  scope                = data.azurerm_subscription.this.id
  role_definition_name = resource.azurerm_role_definition.app_config_settings_read_access[0].name
  principal_id         = var.developers_security_group_object_id
}

# Allow developers to add and remove APIM users to APIM groups on test_001
resource "azurerm_role_assignment" "apim_groups_contributor_access" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = resource.azurerm_role_definition.apim_groups_contributor_access.name
  principal_id         = var.developers_security_group_object_id
}

