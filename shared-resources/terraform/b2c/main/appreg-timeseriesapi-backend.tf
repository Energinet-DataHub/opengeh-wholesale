resource "random_uuid" "backend_timeseriesapi_app_role" {
}

resource "azuread_application" "backend_timeseriesapi_app" {
  display_name    = "backend-timeseriesapi-app"
  identifier_uris = [local.timeseriesapi_id_uri] # NOTE: This should be api://<this-app-id>/backend-timeseriesapi but TF is not able, see: https://github.com/hashicorp/terraform-provider-azuread/issues/428
  owners          = [data.azuread_client_config.current.object_id]

  api {
    requested_access_token_version = 2
  }

  app_role {
    allowed_member_types = ["Application"]
    id                   = random_uuid.backend_timeseriesapi_app_role.result
    value                = "eloverblik"
    display_name         = "ElOverblik"
    description          = "TimeSeriesApi backend Application Scope for ElOverblik."
  }
}

resource "azuread_service_principal" "backend_timeseriesapi_app_sp" {
  client_id               = azuread_application.backend_timeseriesapi_app.client_id
  app_role_assignment_required = false
  owners                       = [data.azuread_client_config.current.object_id]

  feature_tags {
    enterprise = true
  }
}
