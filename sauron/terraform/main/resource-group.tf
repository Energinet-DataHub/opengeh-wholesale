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
  name     = "rg-${local.name_suffix}"
  location = var.location

  tags = local.tags
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

module "pim_contributor_security_group_permissions" {
  count = var.pim_contributor_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=14.22.0"

  resource_group_name = azurerm_resource_group.this.name
  security_group_name = var.pim_contributor_group_name
  role_level          = "Contributor"
  custom_roles_contributor = [
    data.azurerm_role_definition.app_config_settings_read_access.name,
    data.azurerm_role_definition.apim_groups_contributor_access.name,
    data.azurerm_role_definition.locks_contributor_access.name,
  ]
}

module "pim_reader_security_group_permissions" {
  count = var.pim_reader_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=14.22.0"

  resource_group_name = azurerm_resource_group.this.name
  security_group_name = var.pim_reader_group_name
  role_level          = "Reader"
  custom_roles_reader = [
    data.azurerm_role_definition.app_config_settings_read_access.name
  ]
}
