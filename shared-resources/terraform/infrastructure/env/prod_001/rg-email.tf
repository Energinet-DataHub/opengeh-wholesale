module "pim_contributor_security_group_permissions_email" {
  count = var.pim_contributor_data_plane_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_4.0.1"

  resource_group_name = "rg-email-shres-p-we-001"
  security_group_name = var.pim_contributor_data_plane_group_name
  role_level          = "Contributor Data Plane"

  depends_on = [azurerm_resource_group.this]
}

module "pim_contributor_control_plane_security_group_permissions_email" {
  count = var.pim_contributor_control_plane_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_4.0.1"

  resource_group_name = "rg-email-shres-p-we-001"
  security_group_name = var.pim_contributor_control_plane_group_name
  role_level          = "Contributor Control Plane"
  custom_roles_contributor_control_plane = [
    azurerm_role_definition.app_config_settings_read_access.name,
    azurerm_role_definition.apim_groups_contributor_access.name,
    azurerm_role_definition.locks_contributor_access.name,
  ]
}

module "pim_reader_security_group_permissions_email" {
  count = var.pim_reader_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_4.0.1"

  resource_group_name = "rg-email-shres-p-we-001"
  security_group_name = var.pim_reader_group_name
  role_level          = "Reader"
  custom_roles_reader = [
    azurerm_role_definition.app_config_settings_read_access.name
  ]
}
