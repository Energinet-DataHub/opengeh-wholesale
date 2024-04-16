##################
# Old Azure Native security groups
##################

# Give developers dataplane access to dev_001

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

# Allow developers to read config settings in Function apps and App Services on dev_001
resource "azurerm_role_assignment" "app_config_settings_read_access" {
  count                = 1
  scope                = data.azurerm_subscription.this.id
  role_definition_name = resource.azurerm_role_definition.app_config_settings_read_access[0].name
  principal_id         = var.developers_security_group_object_id
}


# Key vault RBAC
resource "azurerm_role_assignment" "developers_key_vault_secrets_user" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = var.developers_security_group_object_id
}

resource "azurerm_role_assignment" "developers_key_vault_cert_user" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Certificate User"
  principal_id         = var.developers_security_group_object_id
}

resource "azurerm_role_assignment" "developers_key_vault_reader" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Reader"
  principal_id         = var.developers_security_group_object_id
}

resource "azurerm_role_assignment" "developers_key_vault_keys_user" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Crypto User"
  principal_id         = var.developers_security_group_object_id
}

##################
# New Omada controlled security groups
##################

# Give developers dataplane access to dev_001

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

# Allow developers to read config settings in Function apps and App Services on dev_001
resource "azurerm_role_assignment" "omada_app_config_settings_read_access" {
  count                = 1
  scope                = data.azurerm_subscription.this.id
  role_definition_name = resource.azurerm_role_definition.app_config_settings_read_access[0].name
  principal_id         = var.omada_developers_security_group_object_id
}


# Key vault RBAC
resource "azurerm_role_assignment" "omada_developers_key_vault_secrets_user" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = var.omada_developers_security_group_object_id
}

resource "azurerm_role_assignment" "omada_developers_key_vault_cert_user" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Certificate User"
  principal_id         = var.omada_developers_security_group_object_id
}

resource "azurerm_role_assignment" "omada_developers_key_vault_reader" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Reader"
  principal_id         = var.omada_developers_security_group_object_id
}

resource "azurerm_role_assignment" "omada_developers_key_vault_keys_user" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Crypto User"
  principal_id         = var.omada_developers_security_group_object_id
}

# Temporary so NHQ can remove locks for CA
resource "azurerm_role_assignment" "nhq_locks_fix" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "User Access Administrator"
  principal_id         = "cc31804d-be36-486b-8a35-c24ae806385c"
}
