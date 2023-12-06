# Give all platformteam developers Control + Dataplane Contributor access to test_002

resource "azurerm_role_assignment" "developers_subscription_contributor" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Contributor"
  principal_id         = var.platform_team_security_group_object_id
}

resource "azurerm_role_assignment" "developers_blob_contributor_access" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.platform_team_security_group_object_id
}

# Give developers dataplane read access to test_002

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


# Allow developers to read config settings in Function apps and App Services on test_002
resource "azurerm_role_assignment" "app_config_settings_read_access" {
  count                = 1
  scope                = data.azurerm_subscription.this.id
  role_definition_name = resource.azurerm_role_definition.app_config_settings_read_access[0].name
  principal_id         = var.developers_security_group_object_id
}
