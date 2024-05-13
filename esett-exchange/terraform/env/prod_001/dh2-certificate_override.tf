resource "azurerm_key_vault_certificate" "esett_dh2_certificate" {
  certificate {
    contents = filebase64("${path.module}/assets/esett_dh2_prod_datahub3_dk.pfx")
    password = var.cert_esett_dh2_datahub3_password
  }
}
