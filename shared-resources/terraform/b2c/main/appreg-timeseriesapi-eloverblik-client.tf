resource "time_sleep" "wait_60_seconds" {
  create_duration = "60s"
}

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

resource "null_resource" "grant_admin_consent" {
  triggers = {
    resourceId = azuread_service_principal.backend_timeseriesapi_app_sp.object_id # resource.azuread_application.backend_timeseriesapi_app.application_id
    clientId   = azuread_service_principal.eloverblik_timeseriesapi_client_app_sp.object_id
    appRoleId  = resource.random_uuid.backend_timeseriesapi_app_role.result
  }
  provisioner "local-exec" {
    command = <<-GRANTCONSENTCMD
        az login --service-principal -u "${var.b2c_client_id}" -p "${var.b2c_client_secret}" --tenant "${var.b2c_tenant_id}" --allow-no-subscriptions
        az rest --method POST \
          --uri 'https://graph.microsoft.com/v1.0/servicePrincipals/${self.triggers.clientId}/appRoleAssignments' \
          --headers 'Content-Type=application/json' \
          --body '{
            "principalId": "${self.triggers.clientId}",
            "resourceId": "${self.triggers.resourceId}",
            "appRoleId": "${self.triggers.appRoleId}"
          }'
        GRANTCONSENTCMD
  }

  depends_on = [
    azuread_service_principal.backend_timeseriesapi_app_sp,
    azuread_service_principal.eloverblik_timeseriesapi_client_app_sp,
    resource.random_uuid.backend_timeseriesapi_app_role,
    time_sleep.wait_60_seconds
  ]
}
