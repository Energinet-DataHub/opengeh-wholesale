locals {
  get_region_code = {
    "North Europe" = "ne"
    "West Europe"  = "we"
    "northeurope"  = "ne"
    "westeurope"   = "we"
    # Add more mappings as needed
    # Terraform currently changes between West Europe and westeurope, which is why both are added
  }
  region_code = local.get_region_code[var.location]
}

data "azurerm_client_config" "this" {}

resource "azurerm_key_vault" "this" {
  name                            = "kv${lower(var.project_name)}${lower(var.environment_short)}${local.region_code}${lower(var.environment_instance)}"
  location                        = var.location
  resource_group_name             = var.resource_group_name
  tenant_id                       = data.azurerm_client_config.this.tenant_id
  sku_name                        = var.sku_name
  enabled_for_template_deployment = true
  enabled_for_deployment          = var.enabled_for_deployment

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}

resource "azurerm_key_vault_access_policy" "selfpermissions" {
  key_vault_id = azurerm_key_vault.this.id
  tenant_id    = data.azurerm_client_config.this.tenant_id
  object_id    = data.azurerm_client_config.this.object_id
  secret_permissions = [
    "Delete",
    "List",
    "Get",
    "Set",
    "Purge"
  ]
  key_permissions = [
    "Create",
    "List",
    "Delete",
    "Get",
    "Purge",
    "Update",
    "Rotate",
    "GetRotationPolicy",
    "SetRotationPolicy"
  ]
  certificate_permissions = [
    "Create",
    "List",
    "Delete",
    "Purge",
    "Get",
    "Import",
    "Recover"
  ]
  storage_permissions = [
    "Delete",
    "Get",
    "Set"
  ]
}

resource "azurerm_key_vault_access_policy" "this" {
  count = length(var.access_policies)

  key_vault_id            = azurerm_key_vault.this.id
  tenant_id               = data.azurerm_client_config.this.tenant_id
  object_id               = data.azurerm_client_config.this.object_id
  secret_permissions      = try(var.access_policies[count.index].secret_permissions, [])
  key_permissions         = try(var.access_policies[count.index].key_permissions, [])
  certificate_permissions = try(var.access_policies[count.index].certificate_permissions, [])
  storage_permissions     = try(var.access_policies[count.index].storage_permissions, [])
}
