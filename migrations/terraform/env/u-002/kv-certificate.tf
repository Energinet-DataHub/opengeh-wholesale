resource "azurerm_app_service_certificate" "dh2_certificate_app" {
  name                = "cert-pwd-migration-dh2-authentication-app"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  pfx_blob            = filebase64("${path.module}/assets/CERT_PWD_MIGRATION_DH2_AUTHENTICATION.pfx")
  app_service_plan_id = data.azurerm_key_vault_secret.plan_shared_id.value
  password            = var.cert_pwd_migration_dh2_authentication_key1
}
