resource "azurerm_role_assignment" "micrsoft_azure_website_access" {
  count                = var.developers_security_group_object_id == null ? 0 : 1
  scope                = module.kv_internal.id
  principal_id         = "e383250e-d5d6-45b3-89f1-5321b821b063"
  role_definition_name = "Key Vault Secrets User"
}

resource "azurerm_role_assignment" "developer_access" {
  count                = var.developers_security_group_object_id == null ? 0 : 1
  scope                = module.kv_internal.id
  principal_id         = var.developers_security_group_object_id
  role_definition_name = "Key Vault Secrets User"
}
