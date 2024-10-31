resource "azurerm_eventhub_namespace" "this" {
  name                         = "evhns-${local.resource_suffix_with_dash}"
  location                     = azurerm_resource_group.this.location
  resource_group_name          = azurerm_resource_group.this.name
  sku                          = "Standard"
  capacity                     = 1
  local_authentication_enabled = false

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

resource "azurerm_role_assignment" "evhns_self" {
  scope                = azurerm_eventhub_namespace.this.id
  role_definition_name = "Azure Event Hubs Data Owner"
  principal_id         = data.azurerm_client_config.this.object_id
}

resource "azurerm_role_assignment" "evhns_spn_ci" {
  scope                = azurerm_eventhub_namespace.this.id
  role_definition_name = "Azure Event Hubs Data Owner"
  principal_id         = azuread_service_principal.spn_ci.object_id
}

resource "azurerm_role_assignment" "evhns_developers" {
  scope                = azurerm_eventhub_namespace.this.id
  role_definition_name = "Azure Event Hubs Data Owner"
  principal_id         = var.omada_developers_security_group_object_id
}

resource "azurerm_key_vault_secret" "kvs_evhns_namespace" {
  name         = "AZURE-EVENTHUB-NAMESPACE"
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
