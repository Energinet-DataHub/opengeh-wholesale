resource "random_uuid" "backend_b2b_app_role" {
  for_each = local.app_roles
}

resource "azuread_application" "backend_b2b_app" {
  display_name    = "backend-b2b-app"
  identifier_uris = [local.b2b_id_uri] # NOTE: This should be api://<this-app-id>/backend-b2b but TF is not able, see: https://github.com/hashicorp/terraform-provider-azuread/issues/428
  owners          = [data.azuread_client_config.current.object_id]

  api {
    requested_access_token_version = 2
  }

  dynamic "app_role" {
    for_each = local.app_roles
    content {
      allowed_member_types = ["Application"]
      id                   = random_uuid.backend_b2b_app_role[app_role.key].result
      value                = app_role.value.role
      display_name         = app_role.value.display_name
      description          = "An application scope representing ${app_role.value.display_name} market role."
    }
  }
}

resource "azuread_service_principal" "backend_b2b_app_sp" {
  client_id                    = azuread_application.backend_b2b_app.client_id
  app_role_assignment_required = false
  owners                       = [data.azuread_client_config.current.object_id]

  feature_tags {
    enterprise = true
  }
}
