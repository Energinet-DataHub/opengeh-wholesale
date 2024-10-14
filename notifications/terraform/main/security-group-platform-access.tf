#
# Assign platform security group access to the resource group - based on configuration of environment
#
module "platform_security_group_permissions_contributor" {
  count = var.platform_security_group_contributor_access == true ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_5.0.0"

  resource_group_name = azurerm_resource_group.this.name
  security_group_name = var.platform_security_group_name
  role_level          = "Contributor"
  custom_roles_contributor = [
    data.azurerm_role_definition.apim_groups_contributor_access.name,
    data.azurerm_role_definition.locks_contributor_access.name,
  ]

 depends_on = [azurerm_resource_group.this]
}

module "platform_security_group_permissions_reader" {
  count = var.platform_security_group_reader_access == true ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_5.0.0"

  resource_group_name = azurerm_resource_group.this.name
  security_group_name = var.platform_security_group_name
  role_level          = "Reader"
  custom_roles_reader = [
    data.azurerm_role_definition.app_config_settings_read_access.name
  ]

  depends_on = [azurerm_resource_group.this]
}
