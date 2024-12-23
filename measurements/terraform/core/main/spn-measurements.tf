data "azuread_client_config" "current" {}

resource "azuread_application" "app_databricks" {
  display_name = "sp-databricks-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  owners = [
    data.azuread_client_config.current.object_id
  ]
}

resource "azuread_service_principal" "spn_databricks" {
  client_id                    = azuread_application.app_databricks.client_id
  app_role_assignment_required = false
  owners = [
    data.azuread_client_config.current.object_id
  ]
}

# see: https://2mas.github.io/blog/rotating-azure-app-registration-secrets-with-terraform/
resource "time_rotating" "first" {
  rotation_months = 12
}

# Expires at time_rotating.first + 6 months
resource "time_rotating" "second" {
  # This is the base timestamp to use
  rfc3339         = time_rotating.first.rotation_rfc3339
  rotation_months = 6

  lifecycle {
    ignore_changes = [rfc3339]
  }
}

# Secret is valid 24 months but will be rotated after 6 months
resource "azuread_application_password" "rotating1" {
  display_name   = "terraform-generated-rotating-1"
  application_id = azuread_application.app_databricks.id

  rotate_when_changed = {
    rotation = time_rotating.first.id
  }
}

# Secret is valid 24 months but will be rotated after 6 months
resource "azuread_application_password" "rotating2" {
  display_name   = "terraform-generated-rotating-2"
  application_id = azuread_application.app_databricks.id

  rotate_when_changed = {
    rotation = time_rotating.second.id
  }
}

module "kvs_app_databricks_password" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "spn-databricks-secret"
  value        = time_rotating.first.unix > time_rotating.second.unix ? azuread_application_password.rotating1.value : azuread_application_password.rotating2.value
  key_vault_id = module.kv_internal.id
}

resource "azurerm_role_assignment" "spn_measurement_transactions_eventhub_recevier" {
  scope                = data.azurerm_key_vault_secret.evh_measurement_transactions_id.value
  role_definition_name = "Azure Event Hubs Data Receiver"
  principal_id         = azuread_service_principal.spn_databricks.object_id
}

resource "azurerm_role_assignment" "spn_measurement_transactions_eventhub_sender" {
  scope                = data.azurerm_key_vault_secret.evh_measurement_transactions_id.value
  role_definition_name = "Azure Event Hubs Data Sender"
  principal_id         = azuread_service_principal.spn_databricks.object_id
}
