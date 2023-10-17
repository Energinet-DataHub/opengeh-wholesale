resource "azurerm_key_vault_certificate" "dh2_certificate" {
  certificate {
    contents = filebase64("${path.module}/assets/PROD_CERT_PWD_ESETT_DH2_AUTHENTICATION.pfx")
    password = var.cert_pwd_esett_dh2_authentication_key1
  }
}
