resource "azurerm_key_vault_access_policy" "kv_access_policy_developers_security_group" {
  count         = var.developers_security_group_object_id == null ? 0 : 1

  key_vault_id            = module.kv_shared.id
  tenant_id               = var.arm_tenant_id
  object_id               = var.developers_security_group_object_id
  secret_permissions      = [
    "List",
    "Get"
  ]
  key_permissions         = [
    "Get",
    "List",
    "Sign"
  ]
}