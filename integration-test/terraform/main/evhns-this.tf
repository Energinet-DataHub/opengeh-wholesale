resource "azurerm_eventhub_namespace" "this" {
  name                = "evhns-${local.resource_suffix_with_dash}"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  sku                 = "Standard"
  capacity            = 1
  # TODO: to be disabled when all have updated to new TestCommon where IAM is used
  # local_authentication_enabled = false

  identity {
    type = "SystemAssigned"
  }

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  tags = local.tags
}

resource "azurerm_key_vault_secret" "kvs_evhns_name" {
  name         = "AZURE-EVENTHUB-NAME"
  value        = azurerm_eventhub_namespace.this.name
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_role_assignment.kv_self
  ]
}


# TODO: to be deleted when all have updated to new TestCommon where IAM is used
resource "azurerm_key_vault_secret" "kvs_evhns_connection_string" {
  name         = "AZURE-EVENTHUB-CONNECTIONSTRING"
  value        = azurerm_eventhub_namespace.this.default_primary_connection_string
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_role_assignment.kv_self
  ]
}
