resource "azurerm_key_vault_access_policy" "kv_dh2_certificates_access_policy_app_webapi" {
  key_vault_id = module.kv_dh2_certificates.id

  tenant_id = module.app_webapi.identity.0.tenant_id
  object_id = module.app_webapi.identity.0.principal_id

  certificate_permissions = [
      "Delete",
      "Import",
      "List",
      "Purge",
    ]
}
