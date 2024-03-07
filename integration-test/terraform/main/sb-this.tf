resource "azurerm_servicebus_namespace" "this" {
  name                          = "sb-${local.resource_suffix_with_dash}"
  location                      = azurerm_resource_group.this.location
  resource_group_name           = azurerm_resource_group.this.name
  sku                           = "Premium"
  premium_messaging_partitions  = 1
  capacity                      = 1

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

resource "azurerm_key_vault_secret" "kvs_sbns_connection_string" {
  name         = "AZURE-SERVICEBUS-CONNECTIONSTRING"
  value        = azurerm_servicebus_namespace.this.default_primary_connection_string
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

resource "azurerm_key_vault_secret" "kvs_sbns_namespace" {
  name         = "AZURE-SERVICEBUS-NAMESPACE"
  value        = azurerm_servicebus_namespace.this.name
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
