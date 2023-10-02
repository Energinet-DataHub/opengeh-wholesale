resource "azurerm_key_vault_access_policy" "certificate_permissions" {
  key_vault_id = module.kv_internal.id

  object_id = module.func_timeseriessynchronization.identity.0.principal_id
  tenant_id = module.func_timeseriessynchronization.identity.0.tenant_id

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
  tenant_id    = module.func_timeseriessynchronization.identity.0.tenant_id
  certificate_permissions = [
    "Get"
  ]
  secret_permissions = [
    "Get"
  ]
}

resource "azurerm_key_vault_access_policy" "kv_internal_access_policy_app_time_series_api" {
  key_vault_id = module.kv_internal.id
  tenant_id    = module.app_time_series_api.identity.0.tenant_id
  object_id    = module.app_time_series_api.identity.0.principal_id
  secret_permissions = [
    "List",
    "Get"
  ]
}

resource "azurerm_key_vault_access_policy" "kv_access_policy_developers_security_group" {
  count = var.developers_security_group_object_id == null ? 0 : 1

  key_vault_id = module.kv_internal.id
  tenant_id    = var.tenant_id
  object_id    = var.developers_security_group_object_id
  secret_permissions = [
    "List",
    "Get"
  ]
}
