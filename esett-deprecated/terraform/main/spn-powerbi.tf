data "azuread_client_config" "current" {}

resource "azuread_application" "app_powerbi" {
  display_name = "sp-powerbi-${local.name_suffix}"
  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_service_principal" "spn_powerbi" {
  client_id                     = azuread_application.app_powerbi.client_id
  app_role_assignment_required  = false
  owners                        = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_service_principal_password" "secret" {
  service_principal_id = azuread_service_principal.spn_powerbi.object_id
}

module "kvs_spn_powerbi_secret" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "spn-powerbi-secret"
  value        = azuread_service_principal_password.secret.value
  key_vault_id = module.kv_esett.id
}
