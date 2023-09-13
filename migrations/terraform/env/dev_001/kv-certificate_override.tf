resource "azurerm_key_vault_certificate" "dh2_certificate" {
  certificate {
    contents = filebase64("${path.module}/assets/CERT_PWD_MIGRATION_DH2_AUTHENTICATION.pfx")
    password = var.cert_pwd_migration_dh2_authentication_key1
  }
}
