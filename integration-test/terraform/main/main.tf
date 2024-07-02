data "azuread_client_config" "this" {}
data "azurerm_client_config" "this" {}

resource "azurerm_key_vault_secret" "kvs_resource_group_name" {
  name         = "AZURE-SHARED-RESOURCEGROUP"
  value        = azurerm_resource_group.this.name
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}

resource "azurerm_key_vault_secret" "kvs_shared_subscription_id" {
  name         = "AZURE-SHARED-SUBSCRIPTIONID"
  value        = data.azurerm_client_config.this.subscription_id
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}

resource "azurerm_key_vault_secret" "kvs_shared_tenant_id" {
  name         = "AZURE-SHARED-TENANTID"
  value        = data.azurerm_client_config.this.tenant_id
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}
