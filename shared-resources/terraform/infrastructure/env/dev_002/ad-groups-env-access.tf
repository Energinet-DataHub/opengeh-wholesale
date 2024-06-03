# Give all developers Contributor Control + Dataplane access to dev_002

resource "azurerm_role_assignment" "omada_developers_subscription_contributor" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Contributor"
  principal_id         = var.omada_developers_security_group_object_id
}

resource "azurerm_role_assignment" "omada_developers_blob_contributor_access" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.omada_developers_security_group_object_id
}

# Key vault RBAC
resource "azurerm_role_assignment" "omada_developers_key_vault_administrator" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = var.omada_developers_security_group_object_id
}

# Allow developers to read config settings in Function apps and App Services
resource "azurerm_role_assignment" "app_config_settings_omada_developer_group_read_access" {
  count                = 1
  scope                = data.azurerm_subscription.this.id
  role_definition_name = resource.azurerm_role_definition.app_config_settings_read_access[0].name
  principal_id         = var.omada_developers_security_group_object_id
}
