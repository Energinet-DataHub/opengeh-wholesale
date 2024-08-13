data "azurerm_resource_group" "vnet_resource_group" {
  name = data.azurerm_virtual_network.this.resource_group_name
}

resource "azurerm_role_assignment" "dbj_aadadmin" {
  scope                = data.azurerm_resource_group.vnet_resource_group.id
  role_definition_name = "Contributor"
  principal_id         = "4b6a4911-0c21-4c94-a285-596cf66a4db2"
}

resource "azurerm_role_assignment" "nhq_aadadmin" {
  scope                = data.azurerm_resource_group.vnet_resource_group.id
  role_definition_name = "Contributor"
  principal_id         = "cc31804d-be36-486b-8a35-c24ae806385c"
}

data "azurerm_network_security_group" "nsg_apim" {
  name                = "nsg-apim-shres"
  resource_group_name = data.azurerm_virtual_network.this.resource_group_name
}

resource "azurerm_role_assignment" "nsg_experiment_apims_network_contributor" {
  scope                = data.azurerm_network_security_group.nsg_apim.id
  role_definition_name = "Network Contributor"
  principal_id         = "5b5bc6c0-6b71-47d7-953a-02536a5452e7"
}
