resource "azurerm_key_vault_certificate" "biztalk_certificate" {
  name         = "cert-pwd-esett-biztalk-authentication"
  key_vault_id = module.kv_esett.id

  certificate {
    contents = filebase64("${path.module}/assets/DataHubBiztalkClientCert_Preprod.pfx")
    password = var.cert_pwd_esett_biztalk_authentication_key1
  }
}

data "azurerm_key_vault_secret" "biztalk_certificate_secret" {
  name         = azurerm_key_vault_certificate.biztalk_certificate.name
  key_vault_id = module.kv_esett.id
}

resource "azurerm_app_service_certificate" "biztalk_certificate_app" {
  name                = "cert-pwd-esett-biztalk-authentication-app"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  pfx_blob            = data.azurerm_key_vault_secret.biztalk_certificate_secret.value
  app_service_plan_id = data.azurerm_key_vault_secret.plan_shared_id.value
}
