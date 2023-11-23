resource "azurerm_key_vault_access_policy" "kv_internal_access_policy_func_entrypoint_grid_loss_peek" {
  key_vault_id = module.kv_internal.id

  tenant_id = module.func_entrypoint_grid_loss_peek.identity.0.tenant_id
  object_id = module.func_entrypoint_grid_loss_peek.identity.0.principal_id

  secret_permissions = [
    "List",
    "Get"
  ]
}
