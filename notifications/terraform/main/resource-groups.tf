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
  name     = "rg-${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  location = var.location

  tags = local.tags
}

data "azurerm_resource_group" "shared" {
  name = "rg-shres-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
}

data "azurerm_role_definition" "app_config_settings_read_access" {
  name  = "datahub-app-config-settings-read-access-${var.environment_short}-${local.region_code}-${var.environment_instance}"
  scope = data.azurerm_subscription.this.id
}

data "azurerm_role_definition" "apim_groups_contributor_access" {
  name  = "datahub-apim-groups-contributor-access-${var.environment_short}-${local.region_code}-${var.environment_instance}"
  scope = data.azurerm_subscription.this.id
}

data "azurerm_role_definition" "locks_contributor_access" {
  name  = "datahub-locks-contributor-access-${var.environment_short}-${local.region_code}-${var.environment_instance}"
  scope = data.azurerm_subscription.this.id
}

data "azurerm_role_definition" "app_manage_contributor" {
  name  = "datahub-app-manage-contributor-access-${var.environment_short}-${local.region_code}-${var.environment_instance}"
  scope = data.azurerm_subscription.this.id
}

module "pim_contributor_security_group_permissions" {
  count = var.pim_contributor_data_plane_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_5.0.0"

  resource_group_name = azurerm_resource_group.this.name
  security_group_name = var.pim_contributor_data_plane_group_name
  role_level          = "Contributor Data Plane"
  custom_roles_contributor_data_plane = [
    data.azurerm_role_definition.app_manage_contributor.name,
    data.azurerm_role_definition.apim_groups_contributor_access.name,
  ]

  depends_on = [azurerm_resource_group.this]
}

module "pim_contributor_control_plane_security_group_permissions" {
  count = var.pim_contributor_control_plane_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_5.0.0"

  resource_group_name = azurerm_resource_group.this.name
  security_group_name = var.pim_contributor_control_plane_group_name
  role_level          = "Contributor Control Plane"
  custom_roles_contributor_control_plane = [
    data.azurerm_role_definition.app_manage_contributor.name,
    data.azurerm_role_definition.apim_groups_contributor_access.name,
    data.azurerm_role_definition.locks_contributor_access.name,
  ]

  depends_on = [azurerm_resource_group.this]
}

module "pim_reader_security_group_permissions" {
  count = var.pim_reader_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_5.0.0"

  resource_group_name = azurerm_resource_group.this.name
  security_group_name = var.pim_reader_group_name
  role_level          = "Reader"
  custom_roles_reader = [
    data.azurerm_role_definition.app_config_settings_read_access.name
  ]

  depends_on = [azurerm_resource_group.this]
}
