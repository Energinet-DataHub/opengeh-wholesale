resource "azurerm_key_vault_certificate" "esett_dh2_certificate" {
  name         = "cert-esett-dh2"
  key_vault_id = module.kv_internal.id

  certificate {
    contents = filebase64("${path.module}/assets/esett_dh2_preprod_datahub3_dk.pfx")
    password = var.cert_esett_dh2_datahub3_password
  }
}

data "azurerm_key_vault_secret" "esett_dh2_certificate_secret" {
  name         = azurerm_key_vault_certificate.esett_dh2_certificate.name
  key_vault_id = module.kv_internal.id
}

resource "azurerm_app_service_certificate" "esett_dh2_certificate_app" {
  name                = "cert-esett-dh2"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  pfx_blob            = data.azurerm_key_vault_secret.esett_dh2_certificate_secret.value
  app_service_plan_id = module.func_service_plan.id
}

module "esett_dh2_certificate_thumbprint" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v14"

  name         = "cert-esett-dh2-thumbprint"
  value        = azurerm_key_vault_certificate.esett_dh2_certificate.thumbprint
  key_vault_id = module.kv_internal.id
}
