# Add backup and access to tfstate storage accounts created in the initialize script

data "azurerm_resource_group" "tfstate" {
  name = "rg-tfs-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
}

data "azurerm_storage_account" "tfstate" {
  name                = "sttfs${lower(var.environment_short)}we${lower(var.environment_instance)}"
  resource_group_name = data.azurerm_resource_group.tfstate.name
}

resource "azurerm_role_assignment" "backup_vault_tfstate" {
  scope                = data.azurerm_storage_account.tfstate.id
  role_definition_name = "Storage Account Backup Contributor"
  principal_id         = module.backup_vault.identity.0.principal_id
}

resource "azurerm_data_protection_backup_instance_blob_storage" "tfstate" {
  name               = data.azurerm_storage_account.tfstate.name
  vault_id           = module.backup_vault.id
  location           = data.azurerm_resource_group.tfstate.location
  storage_account_id = data.azurerm_storage_account.tfstate.id

  backup_policy_id                = module.backup_vault.blob_storage_backup_vaulted_policy_id
  storage_account_container_names = local.tsstate_containers

  depends_on = [
    azurerm_role_assignment.backup_vault_tfstate
  ]
}

data "azurerm_storage_containers" "tfstate" {
  storage_account_id = data.azurerm_storage_account.tfstate.id
}

locals {
  tsstate_containers = data.azurerm_storage_containers.tfstate.containers.*.name
}

# PIM Contributor Control Plane, i.e., Outlaws can apply on 001 environments
# As we have no Outlaws only reader group, Contributor Control Plane is the only group to get reader access to tfstate on preprod and prod
module "pim_contributor_control_plane_security_group_permissions_tfstate" {
  count = var.pim_contributor_control_plane_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_7.0.0"

  resource_group_name = data.azurerm_resource_group.tfstate.name
  security_group_name = var.pim_contributor_control_plane_group_name
  role_level          = "Contributor Control Plane"
  custom_roles_contributor_control_plane = [
    azurerm_role_definition.locks_contributor_access.name,
    "Storage Blob Data Contributor"
  ]

  depends_on = [azurerm_role_definition.locks_contributor_access]
}

# Applied on dev_002 and test_002
module "platform_security_group_permissions_contributor_tfstate" {
  count = var.platform_security_group_contributor_access == true ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_7.0.0"

  resource_group_name = data.azurerm_resource_group.tfstate.name
  security_group_name = var.platform_security_group_name
  role_level          = "Contributor"
  custom_roles_contributor = [
    azurerm_role_definition.locks_contributor_access.name
  ]

  depends_on = [azurerm_role_definition.locks_contributor_access]
}

# Applied on dev and test to give Outlaws reader access by default
resource "azurerm_role_assignment" "tfstate_reader_outlaws" {
  count = var.environment_short == "d" || var.environment_short == "t" ? 1 : 0

  scope                = data.azurerm_storage_account.tfstate.id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = data.azuread_group.platform_security_group_name.object_id
}

resource "azurerm_management_lock" "tfstate_lock" {
  name       = "tfstate-lock"
  scope      = data.azurerm_storage_account.tfstate.id
  lock_level = "CanNotDelete"
}
