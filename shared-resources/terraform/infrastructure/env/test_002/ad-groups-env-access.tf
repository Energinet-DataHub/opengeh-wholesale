# Make all platformteam developers owner of test_002

resource "azurerm_role_assignment" "omada_platform_developers_subscription_owner" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Owner"
  principal_id         = var.omada_platform_team_security_group_object_id
}

# Give all platformteam developers Control + Dataplane Contributor access to test_002

resource "azurerm_role_assignment" "omada_platform_developers_subscription_contributor" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Contributor"
  principal_id         = var.omada_platform_team_security_group_object_id
}

resource "azurerm_role_assignment" "omada_platform_developers_blob_contributor_access" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.omada_platform_team_security_group_object_id
}

# Give developers dataplane read access to test_002

resource "azurerm_role_assignment" "omada_developers_storage_blob_read_access" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = var.omada_developers_security_group_object_id
}

resource "azurerm_role_assignment" "omada_developers_storage_queue_read_access" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Storage Queue Data Reader"
  principal_id         = var.omada_developers_security_group_object_id
}

# Allow developers to read config settings in Function apps and App Services on test_002
resource "azurerm_role_assignment" "app_config_settings_omada_developer_group_read_access" {
  count                = 1
  scope                = data.azurerm_subscription.this.id
  role_definition_name = resource.azurerm_role_definition.app_config_settings_read_access[0].name
  principal_id         = var.omada_developers_security_group_object_id
}

# Key vault RBAC
resource "azurerm_role_assignment" "omada_developers_key_vault_administrator" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = var.omada_developers_security_group_object_id
}
