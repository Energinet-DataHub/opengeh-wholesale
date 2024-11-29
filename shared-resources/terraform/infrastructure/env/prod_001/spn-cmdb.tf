# Service principal for the CMDB application
# It allows the CMDB team to link servicenow tickets to specific resources
resource "azuread_application" "cmdb" {
  display_name = "sp-cmdb-${local.resources_suffix}"
  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000" # Microsoft Graph

    resource_access {
      id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d" # User.Read
      type = "Role"
    }
    resource_access {
      id   = "11d4cd79-5ba5-460f-803f-e22c8ab85ccd" # Device.Read
      type = "Role"
    }
    resource_access {
      id   = "314874da-47d6-4978-88dc-cf0d37f0bb82" # DeviceManagementManagedDevices.Read.All
      type = "Role"
    }
  }
  required_resource_access {
    resource_app_id = "ca7f3f0b-7d91-482c-8e09-c5d840d0eac5" # Log analytics API

    resource_access {
      id   = "e8dac03d-d467-4a7e-9293-9cca7df08b31" # Data.Read
      type = "Role"
    }
  }
  owners = [
    data.azuread_client_config.current_client.object_id
  ]
}

resource "azuread_service_principal" "cmdb" {
  client_id = azuread_application.cmdb.client_id
  owners = [
    data.azuread_client_config.current_client.object_id
  ]
}

resource "azuread_application_password" "cmdb" {
  application_id = azuread_application.cmdb.id
}

module "kvs_spn_cmdb_password" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "spn-cmdb-secret"
  value        = azuread_application_password.cmdb.value
  key_vault_id = module.kv_shared.id
}

resource "azurerm_role_assignment" "spn_cmdb" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Reader"
  principal_id         = azuread_service_principal.cmdb.object_id
}
