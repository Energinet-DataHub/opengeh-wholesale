resource "azurerm_key_vault_certificate" "dh2_certificate" {
  name         = "cert-dh2bridge-dh2-authentication"
  key_vault_id = module.kv_internal.id

  certificate {
    contents = filebase64("${path.module}/assets/CERT_DH2BRIDGE_DH2_AUTHENTICATION.pfx")
    password = var.cert_pwd_dh2bridge_dh2_authentication_key1
  }
}

data "azurerm_key_vault_secret" "dh2_certificate_secret" {
  name         = azurerm_key_vault_certificate.dh2_certificate.name
  key_vault_id = module.kv_internal.id
}

resource "azurerm_app_service_certificate" "dh2_certificate_app" {
  name                = "cert-dh2bridge-dh2-authentication-app"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  pfx_blob            = data.azurerm_key_vault_secret.dh2_certificate_secret.value
  app_service_plan_id = data.azurerm_key_vault_secret.plan_shared_id.value
}
