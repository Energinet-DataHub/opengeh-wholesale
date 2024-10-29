resource "azurerm_servicebus_namespace" "this" {
  name                         = "sb-${local.resource_suffix_with_dash}"
  location                     = azurerm_resource_group.this.location
  resource_group_name          = azurerm_resource_group.this.name
  sku                          = "Premium"
  premium_messaging_partitions = 1
  capacity                     = 1
  # TODO: when all subsystems use RBAC, disable local auth
  # local_auth_enabled           = false

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

resource "azurerm_role_assignment" "sbns_self" {
  scope                = azurerm_servicebus_namespace.this.id
  role_definition_name = "Azure Service Bus Data Owner"
  principal_id         = data.azurerm_client_config.this.object_id
}

resource "azurerm_role_assignment" "sbns_spn_ci" {
  scope                = azurerm_servicebus_namespace.this.id
  role_definition_name = "Azure Service Bus Data Owner"
  principal_id         = azuread_service_principal.spn_ci.object_id
}

resource "azurerm_role_assignment" "sbns_developers" {
  scope                = azurerm_servicebus_namespace.this.id
  role_definition_name = "Azure Service Bus Data Owner"
  principal_id         = var.omada_developers_security_group_object_id
}

resource "azurerm_key_vault_secret" "kvs_sbns_endpoint" {
  name         = "AZURE-SERVICEBUS-ENDPOINT"
  value        = azurerm_servicebus_namespace.this.endpoint
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

# TODO: remove this when subsystems use RBAC
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
    azurerm_role_assignment.kv_self
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
    azurerm_role_assignment.kv_self
  ]
}
