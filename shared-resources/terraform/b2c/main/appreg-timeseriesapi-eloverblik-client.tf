resource "azuread_application" "eloverblik_timeseriesapi_client_app" {
  display_name = "eloverblik-timeseriesapi-client-app"
  owners       = [data.azuread_client_config.current.object_id]

  api {
    requested_access_token_version = 2
  }

  required_resource_access {
    resource_app_id = resource.azuread_application.backend_timeseriesapi_app.application_id

    resource_access {
      id   = resource.random_uuid.backend_timeseriesapi_app_role.result
      type = "Role"
    }
  }
}

resource "azuread_service_principal" "eloverblik_timeseriesapi_client_app_sp" {
  application_id               = azuread_application.eloverblik_timeseriesapi_client_app.application_id
  app_role_assignment_required = false
  owners                       = [data.azuread_client_config.current.object_id]

  feature_tags {
    enterprise = true
  }
}

resource "azuread_application_password" "eloverblik_timeseriesapi_client_secret" {
  application_object_id = azuread_application.eloverblik_timeseriesapi_client_app.object_id
}

resource "null_resource" "grant_admin_consent" {
  triggers = {
    resourceId = azuread_service_principal.backend_timeseriesapi_app_sp.object_id # resource.azuread_application.backend_timeseriesapi_app.application_id
    clientId   = azuread_service_principal.eloverblik_timeseriesapi_client_app_sp.object_id
    scope      = "eloverblik"
  }

  # provisioner "local-exec" {
  #   command = <<-GRANTCONSENTCMD
  #     az rest --method POST \
  #       --uri 'https://graph.microsoft.com/v1.0/oauth2PermissionGrants' \
  #       --headers 'Content-Type=application/json' \
  #       --body '{
  #         "clientId": "${self.triggers.clientId}",
  #         "consentType": "AllPrincipals",
  #         "principalId": null,
  #         "resourceId": "${self.triggers.resourceId}",
  #         "scope": "${self.triggers.scope}"
  #       }'
  #     GRANTCONSENTCMD
  # }
}
