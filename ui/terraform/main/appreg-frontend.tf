resource "azuread_service_principal" "msgraph" {
  application_id = data.azuread_application_published_app_ids.well_known.result.MicrosoftGraph
  use_existing   = true
}

resource "azuread_application" "frontend_app" {
  display_name     = "frontend-app"
  owners           = [data.azuread_client_config.current.object_id]
  sign_in_audience = "AzureADandPersonalMicrosoftAccount"

  # NOTE: https://github.com/hashicorp/terraform-provider-azuread/issues/773
  # There is a third option missing, which is NULL. This needs to be flipped manually in manifest.
  fallback_public_client_enabled = true

  # Implicit flow enabled to allow for acceptance tests in certain environments.
  web {
    implicit_grant {
      access_token_issuance_enabled = true
      id_token_issuance_enabled     = true
    }
  }

  api {
    requested_access_token_version = 2
  }

  single_page_application {
    redirect_uris = ["https://${local.frontend_url}/", "https://localhost/"]
  }

  required_resource_access {
    resource_app_id = data.azurerm_key_vault_secret.backend_bff_app_id.value

    resource_access {
      id   = data.azurerm_key_vault_secret.backend_bff_app_scope_id.value
      type = "Scope"
    }
  }

  required_resource_access {
    resource_app_id = data.azuread_application_published_app_ids.well_known.result.MicrosoftGraph

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["openid"]
      type = "Scope"
    }

    resource_access {
      id   = azuread_service_principal.msgraph.oauth2_permission_scope_ids["offline_access"]
      type = "Scope"
    }
  }
}

resource "azuread_service_principal" "frontend_app_sp" {
  application_id               = azuread_application.frontend_app.application_id
  app_role_assignment_required = false
  owners                       = [data.azuread_client_config.current.object_id]
}

resource "azuread_service_principal_delegated_permission_grant" "grant_frontend_app_bff_admin_consent" {
  service_principal_object_id          = azuread_service_principal.frontend_app_sp.object_id
  resource_service_principal_object_id = data.azurerm_key_vault_secret.backend_bff_app_sp_id.value
  claim_values                         = ["api"]
}

resource "azuread_service_principal_delegated_permission_grant" "grant_frontend_app_openid_admin_consent" {
  service_principal_object_id          = azuread_service_principal.frontend_app_sp.object_id
  resource_service_principal_object_id = azuread_service_principal.msgraph.object_id
  claim_values                         = ["openid", "offline_access"]
}

module "kvs_frontend_app_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "frontend-app-id"
  value        = azuread_application.frontend_app.application_id
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

# There is currently a circular dependency between 'market-participant' and 'ui' deployment.
#   'ui' deployment requires OpenId configuration url from 'market-participant'.
#   'market-participant' deployment requires an invitation link (frontend application registration and static-site url) from 'ui'.
# The dependency is broken by providing the value to 'market-participant' through a key value secret.
module "kvs_b2c_invitation_flow_uri" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "b2c-invitation-flow-uri"
  value        = local.b2c_authorization_invite_uri
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
