locals {
  email_resource_group_name = "rg-email-shres-p-we-001"
}

data "azurerm_resource_group" "email_resource_group" {
  name = "rg-email-shres-p-we-001"
}

module "pim_contributor_control_plane_security_group_permissions_email" {
  count = var.pim_contributor_control_plane_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_5.0.0"

  resource_group_name = data.azurerm_resource_group.email_resource_group.name
  security_group_name = var.pim_contributor_control_plane_group_name
  role_level          = "Contributor Control Plane"
  # We add owner to this resource group, as Owner is required to access SendGrid
  custom_roles_contributor_control_plane = [
    azurerm_role_definition.contributor_app_developers.name,
    azurerm_role_definition.apim_groups_contributor_access.name,
    azurerm_role_definition.locks_contributor_access.name,
    "Owner"
  ]
}

resource "azurerm_management_lock" "sendgrid_lock" {
  name       = "sendgrid-lock"
  scope      = data.azurerm_resource_group.email_resource_group.id
  lock_level = "CanNotDelete"
}
