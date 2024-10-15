# Add backup and access to github tfstate storage account created in dh-githubautomation

data "azurerm_resource_group" "github_tfstate" {
  name = "rg-github-prod-001"
}

data "azurerm_storage_account" "github_tfstate" {
  name                = "stgithubtfspwe001"
  resource_group_name = data.azurerm_resource_group.github_tfstate.name
}

resource "azurerm_role_assignment" "backup_vault_github_tfstate" {
  scope                = data.azurerm_storage_account.github_tfstate.id
  role_definition_name = "Storage Account Backup Contributor"
  principal_id         = module.backup_vault.identity.0.principal_id
}

resource "azurerm_data_protection_backup_instance_blob_storage" "github_tfstate" {
  name               = data.azurerm_storage_account.github_tfstate.name
  vault_id           = module.backup_vault.id
  location           = data.azurerm_resource_group.github_tfstate.location
  storage_account_id = data.azurerm_storage_account.github_tfstate.id

  backup_policy_id                = module.backup_vault.blob_storage_backup_vaulted_policy_id
  storage_account_container_names = local.github_tfstate_containers

  depends_on = [
    azurerm_role_assignment.backup_vault_github_tfstate
  ]
}

data "azurerm_storage_containers" "github_tfstate" {
  storage_account_id = data.azurerm_storage_account.github_tfstate.id
}

locals {
  github_tfstate_containers = data.azurerm_storage_containers.github_tfstate.containers.*.name
}

# PIM Contributor Control Plane, i.e., Outlaws can apply on prod
# As we have no Outlaws only reader group, Contributor Control Plane is the only group to get reader access to github tfstate on prod
module "pim_contributor_control_plane_security_group_permissions_github_tfstate" {
  count = var.pim_contributor_control_plane_group_name != "" ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/resource-group-role-assignments?ref=resource-group-role-assignments_5.0.0"

  resource_group_name = data.azurerm_resource_group.github_tfstate.name
  security_group_name = var.pim_contributor_control_plane_group_name
  role_level          = "Contributor Control Plane"
  custom_roles_contributor_control_plane = [
    azurerm_role_definition.locks_contributor_access.name,
    "Storage Blob Data Contributor"
  ]

  depends_on = [azurerm_role_definition.locks_contributor_access]
}

resource "azurerm_management_lock" "tfstate_github_lock" {
  name       = "github-tfstate-lock"
  scope      = data.azurerm_storage_account.github_tfstate.id
  lock_level = "CanNotDelete"
}
