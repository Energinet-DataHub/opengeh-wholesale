data "azuread_application_published_app_ids" "well_known" {}

data "azuread_service_principal" "msgraph" {
  client_id = data.azuread_application_published_app_ids.well_known.result["MicrosoftGraph"]
}

resource "azuread_application" "bff" {
  provider         = azuread.b2c
  display_name     = "test-bff-${local.resource_suffix_with_dash}"
  sign_in_audience = "AzureADMultipleOrgs"

  api {
    requested_access_token_version = 2
  }

  required_resource_access {
    resource_app_id = data.azuread_application_published_app_ids.well_known.result["MicrosoftGraph"]
    resource_access {
      id   = data.azuread_service_principal.msgraph.oauth2_permission_scope_ids["User.Read"]
      type = "Scope"
    }
  }
}

resource "random_uuid" "backend_b2c_app_role" {
  for_each = local.b2c_backend_app_roles
}

resource "azuread_application" "backend" {
  provider         = azuread.b2c
  display_name     = "test-backend-${local.resource_suffix_with_dash}"
  sign_in_audience = "AzureADMultipleOrgs"

  dynamic "app_role" {
    for_each = local.b2c_backend_app_roles
    content {
      allowed_member_types = ["Application"]
      id                   = random_uuid.backend_b2c_app_role[app_role.key].result
      value                = app_role.value.role
      display_name         = app_role.value.display_name
      description          = "An application scope representing ${app_role.value.display_name} market role."
    }
  }
}

resource "azuread_application" "sp" {
  provider         = azuread.b2c
  display_name     = "sp-b2c-${local.resource_suffix_with_dash}"
  sign_in_audience = "AzureADMultipleOrgs" # Required for B2C
}

resource "azuread_service_principal" "sp" {
  provider  = azuread.b2c
  client_id = azuread_application.sp.client_id
}

resource "azuread_directory_role" "global_admin" {
  provider     = azuread.b2c
  display_name = "Global Administrator"
}

resource "azuread_directory_role_assignment" "assign_global_admin" {
  provider            = azuread.b2c
  principal_object_id = azuread_service_principal.sp.object_id
  role_id             = azuread_directory_role.global_admin.object_id
}

resource "azuread_application_federated_identity_credential" "sp" {
  provider       = azuread.b2c
  application_id = azuread_application.sp.id

  display_name = "test_002"
  subject      = "repo:Energinet-DataHub/dh3-infrastructure:environment:test_002"

  audiences = ["api://AzureADTokenExchange"]
  issuer    = "https://token.actions.githubusercontent.com"
}

resource "azurerm_key_vault_secret" "kvs_azure-b2c-testbff-app-id" {
  name         = "AZURE-B2C-TESTBFF-APP-ID"
  value        = azuread_application.bff.application_id
  key_vault_id = azurerm_key_vault.this.id

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}

resource "azurerm_key_vault_secret" "kvs_azure-b2c-testbackend-app-id" {
  name         = "AZURE-B2C-TESTBACKEND-APP-ID"
  value        = azuread_application.backend.application_id
  key_vault_id = azurerm_key_vault.this.id

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}

resource "azurerm_key_vault_secret" "kvs_azure-b2c-backend-app-objectid" {
  name         = "AZURE-B2C-TESTBACKEND-APP-OBJECTID"
  value        = azuread_application.backend.object_id
  key_vault_id = azurerm_key_vault.this.id

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}

resource "azurerm_key_vault_secret" "kvs_azure-b2c-backend-spn-objectid" {
  name         = "AZURE-B2C-TESTBACKEND-SPN-OBJECTID"
  value        = azuread_service_principal.sp.object_id
  key_vault_id = azurerm_key_vault.this.id

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}

