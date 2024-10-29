resource "azurerm_key_vault" "this" {
  name                      = "kv${local.resource_suffix_without_dash}"
  location                  = azurerm_resource_group.this.location
  resource_group_name       = azurerm_resource_group.this.name
  tenant_id                 = data.azurerm_client_config.this.tenant_id
  sku_name                  = "standard"
  enable_rbac_authorization = true

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

resource "azurerm_role_assignment" "kv_self" {
  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azurerm_client_config.this.object_id
}

resource "azurerm_role_assignment" "kv_spn_ci" {
  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = azuread_service_principal.spn_ci.object_id
}

resource "azurerm_role_assignment" "kv_omada_developer_ad_group" {
  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = var.omada_developers_security_group_object_id
}

locals {
  key_vault_secrets = concat(var.kv_secrets, var.kv_variables)
}

resource "azurerm_key_vault_secret" "kv_secrets" {
  for_each     = { for secret in local.key_vault_secrets : secret.name => secret }
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
    azurerm_role_assignment.kv_self
  ]
}
