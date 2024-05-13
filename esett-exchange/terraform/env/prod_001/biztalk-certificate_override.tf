resource "azurerm_key_vault_certificate" "esett_biztalk_certificate" {
  certificate {
    contents = filebase64("${path.module}/assets/esett_biztalk_prod_datahub3_dk.pfx")
    password = var.cert_esett_biztalk_datahub3_password
  }
}
