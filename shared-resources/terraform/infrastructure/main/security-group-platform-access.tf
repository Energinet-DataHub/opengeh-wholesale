data "azuread_group" "platform_security_group_name" {
  display_name = var.platform_security_group_name
}

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
    azurerm_role_definition.apim_groups_contributor_access.name,
    azurerm_role_definition.locks_contributor_access.name,
  ]
}

module "platform_security_group_permissions_reader" {
  count = var.platform_security_group_reader_access == true ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_5.0.0"

  resource_group_name = azurerm_resource_group.this.name
  security_group_name = var.platform_security_group_name
  role_level          = "Reader"
  custom_roles_reader = [
    azurerm_role_definition.app_config_settings_read_access.name
  ]
}

#
# CCP Network resource group
#
data "azurerm_resource_group" "rg_ccp_network" {
  name = "rg-network-online-${var.environment}-${local.region_code}-${var.environment_instance}"
}

resource "azurerm_role_assignment" "omada_platform_team_network_access" {
  scope                = data.azurerm_resource_group.rg_ccp_network.id
  role_definition_name = var.environment_instance != "001" ? "Contributor" : "Reader"
  principal_id         = data.azuread_group.platform_security_group_name.object_id
}

# Allow platform team to create and manage support tickets to Azure
resource "azurerm_role_assignment" "omada_platform_support_contributor_access" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Support Request Contributor"
  principal_id         = data.azuread_group.platform_security_group_name.object_id
}
