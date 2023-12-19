resource "azurerm_key_vault" "b2c" {
  name                = "kvb2c${local.resource_suffix_without_dash}"
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

resource "azurerm_key_vault_access_policy" "kv_b2csecrets_selfpermission" {
  key_vault_id = azurerm_key_vault.b2c.id
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

resource "azurerm_key_vault_access_policy" "kv_b2csecrets_spn_ci" {
  key_vault_id = azurerm_key_vault.b2c.id
  tenant_id    = data.azurerm_client_config.this.tenant_id
  object_id    = azuread_service_principal.spn_ci.object_id

  secret_permissions = [
    "Get",
    "List",
  ]
}

resource "azurerm_key_vault_access_policy" "kv_b2csecrets_developer_ad_group" {
  key_vault_id = azurerm_key_vault.b2c.id
  tenant_id    = data.azurerm_client_config.this.tenant_id
  object_id    = var.developers_security_group_object_id

  secret_permissions = [
    "Get",
    "List",
  ]
}

locals {
  b2c_key_vault_secrets = concat(var.b2c_kv_secrets, var.b2c_kv_variables)
}

resource "azurerm_key_vault_secret" "b2c_kv_secrets" {
  for_each     = { for secret in local.b2c_key_vault_secrets : secret.name => secret }
  name         = each.value.name
  value        = each.value.value
  key_vault_id = azurerm_key_vault.b2c.id

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
    azurerm_key_vault_access_policy.kv_b2csecrets_selfpermission
  ]
}
