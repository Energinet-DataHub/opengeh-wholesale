resource "random_uuid" "backend_bff_scope_uuid" {
}

resource "azuread_application" "backend_bff_app" {
  display_name    = "backend-bff-app"
  identifier_uris = [local.bff_id_uri] # NOTE: This should be api://<this-app-id>/backend-bff but TF is not able, see: https://github.com/hashicorp/terraform-provider-azuread/issues/428
  owners          = [data.azuread_client_config.current.object_id]

  api {
    requested_access_token_version = 2

    oauth2_permission_scope {
      admin_consent_display_name = "Backend-For-Frontend Application Scope"
      admin_consent_description  = "An application scope for the BFF. A frontend user must request access to this scope to gain access to DH backend."
      id                         = random_uuid.backend_bff_scope_uuid.result
      type                       = "Admin"
      value                      = "api"
    }
  }
}

resource "azuread_service_principal" "backend_bff_app_sp" {
  client_id               = azuread_application.backend_bff_app.client_id
  app_role_assignment_required = false
  owners                       = [data.azuread_client_config.current.object_id]

  feature_tags {
    enterprise = true
  }
}
