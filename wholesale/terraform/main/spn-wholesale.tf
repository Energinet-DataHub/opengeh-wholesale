data "azuread_client_config" "current" {}

resource "azuread_application" "app_databricks" {
  display_name = "sp-databricks-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_service_principal" "spn_databricks" {
  client_id = azuread_application.app_databricks.client_id
  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_application_password" "secret" {
  application_id = azuread_application.app_databricks.id
}

module "kvs_app_databricks_password" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=14.19.1"

  name         = "spn-databricks-secret"
  value        = azuread_application_password.secret.value
  key_vault_id = module.kv_internal.id
}

resource "azurerm_role_assignment" "ra_datalake_contributor" {
  scope                = data.azurerm_key_vault_secret.st_data_lake_id.value
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.id
}
