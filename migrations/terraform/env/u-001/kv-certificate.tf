resource "azurerm_key_vault_certificate" "dh2_certificate" {
  name         = "cert-pwd-migration-dh2-authentication"
  key_vault_id = module.kv_internal.id

  certificate {
    contents = filebase64("${path.module}/assets/CERT_PWD_MIGRATION_DH2_AUTHENTICATION.pfx")
    password = var.cert_pwd_migration_dh2_authentication_key1
  }
}

resource "null_resource" "import_dh2_certificate_to_function_app" {
  triggers = {
    always_run = "${timestamp()}"
  }

  provisioner "local-exec" {
    command     = "az functionapp config ssl import --resource-group ${azurerm_resource_group.this.name} --name ${module.func_migration.name} --key-vault ${module.kv_internal.name} --key-vault-certificate-name ${azurerm_key_vault_certificate.dh2_certificate.name}"
    interpreter = ["pwsh", "-Command"]
  }
  depends_on = [
    azurerm_key_vault_certificate.dh2_certificate
  ]
}
