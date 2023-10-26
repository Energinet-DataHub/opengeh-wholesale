resource "azurerm_key_vault_certificate" "biztalk_certificate" {
  certificate {
    contents = filebase64("${path.module}/assets/DatahubClientCertificate.pfx")
    password = var.cert_pwd_esett_biztalk_authentication_key1
  }
}
