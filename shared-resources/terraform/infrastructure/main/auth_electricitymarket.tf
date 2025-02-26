resource "random_uuid" "electricitymarket_user_impersonation_scope_id" {}

module "kvs_electricitymarket_application_id_uri" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "electricitymarket-application-id-uri"
  value        = "api://datahub3-electricitymarket-${var.environment_short}-${var.environment_instance}"
  key_vault_id = module.kv_shared.id
}

resource "azuread_application" "app_electricitymarket" {
  identifier_uris = [module.kvs_electricitymarket_application_id_uri.value]
  display_name    = "sp-datahub3-electricitymarket-${var.environment_short}-${var.environment_instance}"

  owners = [
    data.azuread_client_config.current_client.object_id
  ]

  api {
    oauth2_permission_scope {
      admin_consent_description  = "Allow the application to access electricitymarket on behalf of the signed-in user."
      admin_consent_display_name = "Allow Access"
      enabled                    = true
      id                         = random_uuid.electricitymarket_user_impersonation_scope_id.result
      type                       = "User"
      user_consent_description   = "Allow the application to access electricitymarket on your behalf."
      user_consent_display_name  = "Allow Access to electricitymarket"
      value                      = "user_impersonation"
    }
  }
}

// Enables authentication delegation - e.g an authorized user can request access token for the API through the CLI
// az account get-access-token --resource 'api://IDENTIFIER-URI'
// az account get-access-token --scope 'api://IDENTIFIER-URI/user_impersonation'
resource "azuread_application_pre_authorized" "electricitymarket_azcli" {
  application_id       = azuread_application.app_electricitymarket.id
  authorized_client_id = "04b07795-8ddb-461a-bbee-02f9e1bf7b46" // Microsoft Azure CLI service principal

  permission_ids = [
    random_uuid.electricitymarket_user_impersonation_scope_id.result
  ]
}

// Enables authentication delegation - e.g an authorized user can request access token for the API through Visual Studio
resource "azuread_application_pre_authorized" "electricitymarket_visualstudio1" {
  application_id       = azuread_application.app_electricitymarket.id
  authorized_client_id = "872cd9fa-d31f-45e0-9eab-6e460a02d1f1" // Visual Studio service principal

  permission_ids = [
    random_uuid.electricitymarket_user_impersonation_scope_id.result
  ]
}

// Enables authentication delegation - e.g an authorized user can request access token for the API through Visual Studio
resource "azuread_application_pre_authorized" "electricitymarket_visualstudio2" {
  application_id       = azuread_application.app_electricitymarket.id
  authorized_client_id = "04f0c124-f2bc-4f59-8241-bf6df9866bbd" // Visual Studio service principal

  permission_ids = [
    random_uuid.electricitymarket_user_impersonation_scope_id.result
  ]
}

// Enables authentication delegation - e.g an authorized user can request access token for the API through Visual Studio Code
resource "azuread_application_pre_authorized" "electricitymarket_vscode" {
  application_id       = azuread_application.app_electricitymarket.id
  authorized_client_id = "aebc6443-996d-45c2-90f0-388ff96faa56" // Visual Studio Code service principal

  permission_ids = [
    random_uuid.electricitymarket_user_impersonation_scope_id.result
  ]
}

resource "azuread_service_principal" "sp_electricitymarket" {
  client_id = azuread_application.app_electricitymarket.client_id
  owners = [
    data.azuread_client_config.current_client.object_id
  ]

  app_role_assignment_required = true

  feature_tags {
    hide       = true
    enterprise = true
  }
}

// Add the deployment service principal to electricitymarket service principal
// This enables the SP to get accesstokens and thus run systemtests against the electricitymarket API
resource "azuread_app_role_assignment" "electricitymarket_deployment_sp_role_assignment" {
  app_role_id         = "00000000-0000-0000-0000-000000000000" // default role
  resource_object_id  = azuread_service_principal.sp_electricitymarket.object_id
  principal_object_id = data.azuread_client_config.current_client.object_id
}

// Add developer group to electricitymarket service principal on test environments
// This enables the user to get accesstokens and thus debug the electricitymarket API
resource "azuread_app_role_assignment" "electricitymarket_developer_sp_role_assignment" {
  count               = var.environment_instance != "001" ? 1 : 0
  app_role_id         = "00000000-0000-0000-0000-000000000000" // default role
  resource_object_id  = azuread_service_principal.sp_electricitymarket.object_id
  principal_object_id = data.azuread_group.developers.object_id
}

module "kvs_electricitymarket_sp_object_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "electricitymarket-sp-object-id"
  value        = azuread_service_principal.sp_electricitymarket.object_id
  key_vault_id = module.kv_shared.id
}
