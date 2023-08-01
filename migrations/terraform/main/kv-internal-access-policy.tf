resource "azurerm_key_vault_access_policy" "certificate_permissions" {
  key_vault_id = module.kv_internal.id

  object_id = module.func_migration.identity.0.principal_id
  tenant_id = module.func_migration.identity.0.tenant_id

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

resource "azurerm_key_vault_access_policy" "allow_certificate_import" {
  key_vault_id = module.kv_internal.id
  object_id    = "e383250e-d5d6-45b3-89f1-5321b821b063"
  tenant_id    = module.func_migration.identity.0.tenant_id
  certificate_permissions = [
    "Get"
  ]
  secret_permissions = [
    "Get"
  ]
}
