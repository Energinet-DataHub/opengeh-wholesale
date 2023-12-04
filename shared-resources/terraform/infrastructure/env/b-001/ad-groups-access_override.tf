data "azurerm_resource_group" "rg_tfstate" {
    count = 0
}

resource "azurerm_role_definition" "deny_dataplane_access_to_tfs_rg" {
    count = 0
}

resource "azurerm_role_assignment" "deny_developer_dataplane_access_to_tfs_rg" {
    count = 0
}
