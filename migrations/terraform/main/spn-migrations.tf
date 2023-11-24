data "azuread_client_config" "current" {}

resource "azuread_application" "app_databricks" {
  display_name = "sp-databricks-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_service_principal" "spn_databricks" {
  application_id = azuread_application.app_databricks.application_id
  app_role_assignment_required = false
  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_application_password" "secret" {
  application_object_id = azuread_application.app_databricks.object_id
}

module "kvs_app_databricks_password" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "spn-databricks-secret"
  value        = azuread_application_password.secret.value
  key_vault_id = module.kv_internal.id
}

resource "azuread_application" "app_cgi_dh2_data_migration" {
  display_name = "sp-cgi-dh2-data-migration-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_service_principal" "spn_cgi_dh2_data_migration" {
  application_id = azuread_application.app_cgi_dh2_data_migration.application_id
  app_role_assignment_required = false
  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_application_password" "dh2_migration_secret" {
  application_object_id = azuread_application.app_cgi_dh2_data_migration.object_id
}
