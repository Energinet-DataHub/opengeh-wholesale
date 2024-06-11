# Should be moved to eSett exchange when eSett deprecated is removed
resource "azurerm_key_vault_certificate" "esett_biztalk_certificate" {
  name         = "cert-esett-biztalk"
  key_vault_id = module.kv_internal.id

  certificate {
    contents = filebase64("${path.module}/assets/esett_biztalk_preprod_datahub3_dk.pfx")
    password = var.cert_esett_biztalk_datahub3_password
  }
}

data "azurerm_key_vault_secret" "esett_biztalk_certificate_secret" {
  name         = azurerm_key_vault_certificate.esett_biztalk_certificate.name
  key_vault_id = module.kv_internal.id
}

resource "azurerm_app_service_certificate" "esett_biztalk_certificate_app" {
  name                = "cert-esett-biztalk"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  pfx_blob            = data.azurerm_key_vault_secret.esett_biztalk_certificate_secret.value
  app_service_plan_id = module.func_service_plan.id

  tags = local.tags
}

module "esett_biztalk_certificate_thumbprint" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=14.0.3"

  name         = "cert-esett-biztalk-thumbprint"
  value        = azurerm_key_vault_certificate.esett_biztalk_certificate.thumbprint
  key_vault_id = module.kv_internal.id
}
