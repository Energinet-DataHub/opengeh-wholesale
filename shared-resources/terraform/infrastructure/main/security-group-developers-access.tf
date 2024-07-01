data "azuread_group" "developer_security_group_name" {
  display_name = var.developer_security_group_name
}

#
# Assign developers access Controlplane Reader access on subscription level
#
resource "azurerm_role_assignment" "omada_developers_subscription_reader" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Reader"
  principal_id         = data.azuread_group.developer_security_group_name.object_id
}

#
# Assign developers security group access to the resource group - based on configuration of environment
#
module "developer_security_group_permissions_contributor" {
  count = var.developer_security_group_contributor_access == true ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=14.28.0"

  resource_group_name = azurerm_resource_group.this.name
  security_group_name = var.developer_security_group_name
  role_level          = "Contributor"
  custom_roles_contributor = [
    azurerm_role_definition.apim_groups_contributor_access.name,
    azurerm_role_definition.locks_contributor_access.name,
  ]
}

module "developer_security_group_permissions_reader" {
  count = var.developer_security_group_reader_access == true ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=14.28.0"

  resource_group_name = azurerm_resource_group.this.name
  security_group_name = var.developer_security_group_name
  role_level          = "Reader"
  custom_roles_reader = [
    azurerm_role_definition.app_config_settings_read_access.name
  ]
}
