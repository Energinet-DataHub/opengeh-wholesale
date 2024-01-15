resource "azuread_application" "app_datalake_contributor" {
  display_name = "sp-datalakecontributor-${local.resources_suffix}"
  owners = [
    data.azuread_client_config.current_client.object_id
  ]
}

resource "azuread_service_principal" "spn_datalake_contributor" {
  application_id = azuread_application.app_datalake_contributor.application_id
  owners = [
    data.azuread_client_config.current_client.object_id
  ]
}

resource "azuread_application_password" "spn_datalake_contributor_secret" {
  application_object_id = azuread_application.app_datalake_contributor.object_id
}

module "kvs_app_datalake_contributor_password" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "spn-datalake-contributor-secret"
  value        = azuread_application_password.spn_datalake_contributor_secret.value
  key_vault_id = module.kv_shared.id
}

module "kvs_app_datalake_contributor_app_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "spn-datalake-contributor-app-id"
  value        = azuread_application.app_datalake_contributor.application_id
  key_vault_id = module.kv_shared.id
}

resource "azurerm_role_assignment" "ra_datalake_contributor" {
  scope                = module.st_data_lake.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_datalake_contributor.id
}
