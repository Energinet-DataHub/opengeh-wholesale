resource "azurerm_key_vault_access_policy" "kv_internal_access_policy_func_entrypoint_marketparticipant" {
  key_vault_id = module.kv_internal.id

  tenant_id = module.func_entrypoint_marketparticipant.identity.0.tenant_id
  object_id = module.func_entrypoint_marketparticipant.identity.0.principal_id

  secret_permissions = [
    "List",
    "Get"
  ]
}

resource "azurerm_key_vault_access_policy" "kv_internal_access_policy_func_entrypoint_certificate_synchronization" {
  key_vault_id = module.kv_internal.id

  tenant_id = module.func_entrypoint_certificate_synchronization.identity.0.tenant_id
  object_id = module.func_entrypoint_certificate_synchronization.identity.0.principal_id

  secret_permissions = [
    "List",
    "Get"
  ]
}

resource "azurerm_key_vault_access_policy" "kv_internal_access_policy_app_webapi" {
  key_vault_id = module.kv_internal.id

  tenant_id = module.app_webapi.identity.0.tenant_id
  object_id = module.app_webapi.identity.0.principal_id

  key_permissions = [
    "List",
    "Get",
    "Sign"
  ]
}
