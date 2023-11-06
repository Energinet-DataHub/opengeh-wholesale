data "azuread_client_config" "current" {}

resource "azuread_application" "app_market_participant" {
  display_name = "sp-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_service_principal" "spn_market_participant" {
  application_id               = azuread_application.app_market_participant.application_id
  app_role_assignment_required = false
  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_application_password" "secret" {
  application_object_id = azuread_application.app_market_participant.object_id
}

module "kvs_app_market_participant_password" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "spn-market-participant-secret"
  value        = azuread_application_password.secret.value
  key_vault_id = module.kv_internal.id
}

resource "azurerm_role_assignment" "spn_market_participant_api_managment_contributor" {
  scope                = data.azurerm_key_vault_secret.apim_instance_id.value
  role_definition_name = "API Management Service Contributor"
  principal_id         = azuread_service_principal.spn_market_participant.object_id
}
