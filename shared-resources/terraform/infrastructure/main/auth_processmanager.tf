resource "random_uuid" "processmanager_user_impersonation_scope_id" {}

module "kvs_processmanager_application_id_uri" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "processmanager-application-id-uri"
  value        = "api://datahub3-processmanager-${var.environment_short}-${var.environment_instance}"
  key_vault_id = module.kv_shared.id
}

resource "azuread_application" "app_processmanager" {
  identifier_uris = [module.kvs_processmanager_application_id_uri.value]
  display_name    = "sp-datahub3-processmanager-${var.environment_short}-${var.environment_instance}"

  owners = [
    data.azuread_client_config.current_client.object_id
  ]

  api {
    oauth2_permission_scope {
      admin_consent_description  = "Allow the application to access processmanager on behalf of the signed-in user."
      admin_consent_display_name = "Allow Access"
      enabled                    = true
      id                         = random_uuid.processmanager_user_impersonation_scope_id.result
      type                       = "User"
      user_consent_description   = "Allow the application to access processmanager on your behalf."
      user_consent_display_name  = "Allow Access to processmanager"
      value                      = "user_impersonation"
    }
  }
}

// Enables authentication delegation - e.g an authorized user can request access token for the API through the CLI
// az account get-access-token --resource 'api://IDENTIFIER-URI'
// az account get-access-token --scope 'api://IDENTIFIER-URI/user_impersonation'
resource "azuread_application_pre_authorized" "processmanager_azcli" {
  application_id       = azuread_application.app_processmanager.id
  authorized_client_id = "04b07795-8ddb-461a-bbee-02f9e1bf7b46" // Microsoft Azure CLI service principal

  permission_ids = [
    random_uuid.processmanager_user_impersonation_scope_id.result
  ]
}

// Enables authentication delegation - e.g an authorized user can request access token for the API through Visual Studio
resource "azuread_application_pre_authorized" "processmanager_visualstudio1" {
  application_id       = azuread_application.app_processmanager.id
  authorized_client_id = "872cd9fa-d31f-45e0-9eab-6e460a02d1f1" // Visual Studio service principal

  permission_ids = [
    random_uuid.processmanager_user_impersonation_scope_id.result
  ]
}

// Enables authentication delegation - e.g an authorized user can request access token for the API through Visual Studio
resource "azuread_application_pre_authorized" "processmanager_visualstudio2" {
  application_id       = azuread_application.app_processmanager.id
  authorized_client_id = "04f0c124-f2bc-4f59-8241-bf6df9866bbd" // Visual Studio service principal

  permission_ids = [
    random_uuid.processmanager_user_impersonation_scope_id.result
  ]
}

// Enables authentication delegation - e.g an authorized user can request access token for the API through Visual Studio Code
resource "azuread_application_pre_authorized" "processmanager_vscode" {
  application_id       = azuread_application.app_processmanager.id
  authorized_client_id = "aebc6443-996d-45c2-90f0-388ff96faa56" // Visual Studio Code service principal

  permission_ids = [
    random_uuid.processmanager_user_impersonation_scope_id.result
  ]
}

resource "azuread_service_principal" "sp_process_manager" {
  client_id = azuread_application.app_processmanager.client_id
  owners = [
    data.azuread_client_config.current_client.object_id
  ]

  app_role_assignment_required = true

  feature_tags {
    hide       = true
    enterprise = true
  }
}

// Add the deployment service principal to processmanager service principal
// This enables the SP to get accesstokens and thus run systemtests against the processmanager API
resource "azuread_app_role_assignment" "processmanager_deployment_sp_role_assignment" {
  app_role_id         = "00000000-0000-0000-0000-000000000000" // default role
  resource_object_id  = azuread_service_principal.sp_process_manager.object_id
  principal_object_id = data.azuread_client_config.current_client.object_id
}

// Add PIM Contributor group or developer group to processmanager service principal
// This enables the groups members to get accesstokens and thus debug the processmanager API
resource "azuread_app_role_assignment" "processmanager_pim_sp_role_assignment" {
  app_role_id         = "00000000-0000-0000-0000-000000000000" // default role
  resource_object_id  = azuread_service_principal.sp_process_manager.object_id
  principal_object_id = var.environment_instance == "001" ? data.azuread_group.pim_contributor_data_plane_group[0].object_id : data.azuread_group.developers.object_id
}

module "kvs_processmanager_sp_object_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "processmanager-sp-object-id"
  value        = azuread_service_principal.sp_process_manager.object_id
  key_vault_id = module.kv_shared.id
}
