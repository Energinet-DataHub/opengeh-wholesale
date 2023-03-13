locals {
  app_roles = {
    balanceresponsibleparty = {
      role         = "balanceresponsibleparty"
      display_name = "Balance Responsible Party"
    }
    metereddataadministrator = {
      role         = "metereddataadministrator"
      display_name = "Metered Data Administrator"
    }
  }
}

data "azuread_client_config" "current" {}

resource "random_uuid" "backend_b2b_app_role" {
  for_each = local.app_roles
}

resource "azuread_application" "backend_b2b_app" {
  display_name = "backend-b2b-app"
  owners = [
    data.azuread_client_config.current.object_id
  ]

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
      description          = "An application scope representing ${app_role.value.display_name} marked role."
    }
  }
}
