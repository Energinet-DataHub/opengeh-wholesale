locals {
  email_resource_group_name = "rg-email-shres-p-we-001"
}

data "azurerm_resource_group" "email_resource_group" {
  name = "rg-email-shres-p-we-001"
}

resource "azurerm_role_assignment" "xrtni_owner" {
  scope                = data.azurerm_resource_group.email_resource_group.id
  role_definition_name = "Owner"
  principal_id         = "72dac0b0-db26-4fe5-a8f8-d6e34da67f87"
}

module "pim_contributor_security_group_permissions_email" {
  count = var.pim_contributor_data_plane_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_4.2.0"

  resource_group_name = data.azurerm_resource_group.email_resource_group.name
  security_group_name = var.pim_contributor_data_plane_group_name
  role_level          = "Contributor Data Plane"
  custom_roles_contributor_data_plane = [
    azurerm_role_definition.contributor_app_developers.name,
    azurerm_role_definition.apim_groups_contributor_access.name,
  ]

  depends_on = [azurerm_resource_group.this]
}

module "pim_contributor_control_plane_security_group_permissions_email" {
  count = var.pim_contributor_control_plane_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_4.2.0"

  resource_group_name = data.azurerm_resource_group.email_resource_group.name
  security_group_name = var.pim_contributor_control_plane_group_name
  role_level          = "Contributor Control Plane"
  custom_roles_contributor_control_plane = [
    azurerm_role_definition.contributor_app_developers.name,
    azurerm_role_definition.apim_groups_contributor_access.name,
    azurerm_role_definition.locks_contributor_access.name,
  ]
}

module "pim_reader_security_group_permissions_email" {
  count = var.pim_reader_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_4.2.0"

  resource_group_name = data.azurerm_resource_group.email_resource_group.name
  security_group_name = var.pim_reader_group_name
  role_level          = "Reader"
  custom_roles_reader = [
    azurerm_role_definition.app_config_settings_read_access.name
  ]
}
