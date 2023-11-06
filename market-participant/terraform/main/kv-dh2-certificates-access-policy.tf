resource "azurerm_key_vault_access_policy" "kv_dh2_certificates_access_policy_app_webapi" {
  key_vault_id = module.kv_dh2_certificates.id

  tenant_id = module.app_webapi.identity.0.tenant_id
  object_id = module.app_webapi.identity.0.principal_id

  secret_permissions = [
      "Recover",
      "Delete",
      "Set",
      "List",
    ]
}

resource "azurerm_key_vault_access_policy" "kv_dh2_certificates_access_policy_func_entrypoint_certificate_synchronization" {
  key_vault_id = module.kv_dh2_certificates.id

  tenant_id = module.func_entrypoint_certificate_synchronization.identity.0.tenant_id
  object_id = module.func_entrypoint_certificate_synchronization.identity.0.principal_id

  secret_permissions = [
      "Purge",
      "List",
    ]
}

resource "azurerm_key_vault_access_policy" "kv_dh2_certificates_access_policy_apim" {
  key_vault_id = module.kv_dh2_certificates.id

  tenant_id = data.azurerm_subscription.this.tenant_id
  object_id = data.azurerm_key_vault_secret.apim_principal_id.value

  secret_permissions = [
      "Get",
      "List",
    ]
}
