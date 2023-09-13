# Crazy hack for Terraform
# Setting `count` on this resource will make terraform explode, using the variable will make terraform explode
# Error:
# Because data.azuread_group.datalake_readeraccess_group_name has "count"
# set, its attributes must be accessed on specific instances.
# For example, to correlate with indices of a referring resource, use:
# data.azuread_group.datalake_readeraccess_group_name[count.index]
data "azuread_group" "datalake_readeraccess_group_name" {
  display_name = "WILL_NOT_BE_USED"
}

resource "azurerm_role_assignment" "datalake_readeraccess_group_name" {
  count = 0
}

resource "azurerm_role_assignment" "dh2data_readeraccess_group_name" {
  count = 0
}

resource "azurerm_role_assignment" "dh2dropzone_readeraccess_group_name" {
  count = 0
}

resource "azurerm_role_assignment" "dh2dropzonearch_readeraccess_group_name" {
  count = 0
}
