resource "azurerm_resource_group" "this" {
  name     = "rg-${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  location = "West Europe"

  tags = local.tags
}

data "azurerm_resource_group" "shared" {
  name = "rg-shres-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
}

data "azurerm_role_definition" "app_config_settings_read_access" {
  name  = "datahub-app-config-settings-read-access-${var.environment_short}-${var.region_short}-${var.environment_instance}"
  scope = data.azurerm_subscription.this.id
}

data "azurerm_role_definition" "apim_groups_contributor_access" {
  name  = "datahub-apim-groups-contributor-access-${var.environment_short}-${var.region_short}-${var.environment_instance}"
  scope = data.azurerm_subscription.this.id
}

data "azurerm_role_definition" "locks_contributor_access" {
  name  = "datahub-locks-contributor-access-${var.environment_short}-${var.region_short}-${var.environment_instance}"
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
    data.azurerm_role_definition.app_config_settings_read_access.name,
  ]
}
