resource "azurerm_key_vault" "this" {
  name                = "kv${local.resource_suffix_without_dash}"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  tenant_id           = data.azurerm_client_config.this.tenant_id
  sku_name            = "standard"

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

resource "azurerm_key_vault_access_policy" "kv_selfpermission" {
  key_vault_id = azurerm_key_vault.this.id
  tenant_id    = data.azurerm_client_config.this.tenant_id
  object_id    = data.azurerm_client_config.this.object_id

  secret_permissions = [
    "Delete",
    "List",
    "Get",
    "Set",
    "Purge",
  ]
}

resource "azurerm_key_vault_access_policy" "kv_spn_ci" {
  key_vault_id = azurerm_key_vault.this.id
  tenant_id    = data.azurerm_client_config.this.tenant_id
  object_id    = azuread_service_principal.spn_ci.object_id

  secret_permissions = [
    "Get",
    "Set",
    "List",
    "Delete",
  ]

  key_permissions = [
    "Get",
    "List",
    "Update",
    "Create",
    "Delete",
    "Sign",
  ]
}

resource "azurerm_key_vault_access_policy" "kv_developer_ad_group" {
  key_vault_id = azurerm_key_vault.this.id
  tenant_id    = data.azurerm_client_config.this.tenant_id
  object_id    = var.developers_security_group_object_id

  secret_permissions = [
    "Get",
    "Set",
    "List",
    "Delete",
  ]

  key_permissions = [
    "Get",
    "List",
    "Update",
    "Create",
    "Delete",
    "Sign",
  ]
}

resource "azurerm_key_vault_secret" "kv_secrets" {
  for_each     = { for secret in var.kv_secrets : secret.name => secret }
  name         = each.value.name
  value        = each.value.value
  key_vault_id = azurerm_key_vault.this.id

  timeouts {
    read = "15m"
  }

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }

  # Ensure the access policy was created before trying to access the Key Vault
  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}
