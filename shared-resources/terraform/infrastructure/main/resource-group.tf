locals {
  get_region_code = {
    "North Europe" = "ne"
    "West Europe"  = "we"
    "northeurope"  = "ne"
    "westeurope"   = "we"
    # Add more mappings as needed
    # Terraform currently changes between West Europe and westeurope, which is why both are added
  }
  region_code = local.get_region_code[var.location]
}

resource "azurerm_resource_group" "this" {
  name     = "rg-${local.resources_suffix}"
  location = var.location

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  tags = local.tags
}

module "pim_contributor_security_group_permissions" {
  count = var.pim_contributor_data_plane_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_5.0.0"

  resource_group_name = azurerm_resource_group.this.name
  security_group_name = var.pim_contributor_data_plane_group_name
  role_level          = "Contributor Data Plane"
  custom_roles_contributor_data_plane = [
    azurerm_role_definition.contributor_app_developers.name,
    azurerm_role_definition.apim_groups_contributor_access.name,
  ]

  depends_on = [azurerm_resource_group.this, azurerm_role_definition.contributor_app_developers, azurerm_role_definition.apim_groups_contributor_access]
}

module "pim_contributor_control_plane_security_group_permissions" {
  count = var.pim_contributor_control_plane_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_5.0.0"

  resource_group_name = azurerm_resource_group.this.name
  security_group_name = var.pim_contributor_control_plane_group_name
  role_level          = "Contributor Control Plane"
  custom_roles_contributor_control_plane = [
    azurerm_role_definition.contributor_app_developers.name,
    azurerm_role_definition.apim_groups_contributor_access.name,
    azurerm_role_definition.locks_contributor_access.name,
  ]

  depends_on = [azurerm_role_definition.contributor_app_developers, azurerm_role_definition.apim_groups_contributor_access, azurerm_role_definition.locks_contributor_access]
}

module "pim_reader_security_group_permissions" {
  count = var.pim_reader_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_5.0.0"

  resource_group_name = azurerm_resource_group.this.name
  security_group_name = var.pim_reader_group_name
  role_level          = "Reader"
  custom_roles_reader = [
    azurerm_role_definition.app_config_settings_read_access.name
  ]

  depends_on = [azurerm_role_definition.app_config_settings_read_access]
}
