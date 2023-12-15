resource "azurerm_key_vault_certificate" "b2b_datahub3_certificate" {
  name         = "cert-b2b-datahub3"
  key_vault_id = module.kv_shared.id

  certificate {
    contents = filebase64("${path.module}/assets/b2b_datahub3_dk.pfx")
    password = var.cert_b2b_datahub3_password
  }
}

resource "azurerm_key_vault_certificate" "ebix_datahub3_certificate" {
  name         = "cert-ebix-datahub3"
  key_vault_id = module.kv_shared.id

  certificate {
    contents = filebase64("${path.module}/assets/ebix_datahub3_dk.pfx")
    password = var.cert_ebix_datahub3_password
  }
}

resource "azurerm_api_management_custom_domain" "datahub3_custom_domains" {
  api_management_id = module.apim_shared.id

  gateway {
    host_name    = "b2b.datahub3.dk"
    key_vault_id = azurerm_key_vault_certificate.b2b_datahub3_certificate.versionless_secret_id
  }

  gateway {
    host_name    = "ebix.datahub3.dk"
    key_vault_id = azurerm_key_vault_certificate.ebix_datahub3_certificate.versionless_secret_id
  }
}
