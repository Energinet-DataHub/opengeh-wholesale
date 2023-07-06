resource "azurerm_key_vault_access_policy" "certificate_permissions" {
  key_vault_id = module.kv_internal.id

  object_id    = module.func_migration.identity.0.principal_id
  tenant_id    = module.func_migration.identity.0.tenant_id

  certificate_permissions = [
    "Get",
    "Create",
    "Delete",
    "List",
    "Import",
    "Update",
    "Purge"
  ]

  secret_permissions = [
    "Get"
  ]
}
