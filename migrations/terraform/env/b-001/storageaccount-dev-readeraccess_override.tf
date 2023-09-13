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
