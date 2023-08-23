resource "azurerm_key_vault_access_policy" "kv_shared_access_policy_app_time_series_api" {
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
  tenant_id    = module.app_time_series_api.identity.0.tenant_id
  object_id    = module.app_time_series_api.identity.0.principal_id
  secret_permissions = [
    "List",
    "Get"
  ]
}
