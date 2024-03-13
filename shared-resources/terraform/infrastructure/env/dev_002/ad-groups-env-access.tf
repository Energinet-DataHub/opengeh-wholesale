# Give all developers Contributor Control + Dataplane access to dev_002

resource "azurerm_role_assignment" "developers_subscription_contributor" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Contributor"
  principal_id         = var.developers_security_group_object_id
}

resource "azurerm_role_assignment" "developers_blob_contributor_access" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.developers_security_group_object_id
}

# Key vault RBAC
resource "azurerm_role_assignment" "developers_key_vault_secrets_officer" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = var.developers_security_group_object_id
}

resource "azurerm_role_assignment" "developers_key_vault_cert_officer" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Certificates Officer"
  principal_id         = var.developers_security_group_object_id
}

resource "azurerm_role_assignment" "developers_key_vault_administrator" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = var.developers_security_group_object_id
}
