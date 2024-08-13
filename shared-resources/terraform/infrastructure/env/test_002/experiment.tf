data "azurerm_network_security_group" "nsg_apim" {
  name                = "nsg-apim-shres"
  resource_group_name = data.azurerm_virtual_network.this.resource_group_name
}

resource "azurerm_role_assignment" "nsg_experiment_apims_network_contributor" {
  scope                = data.azurerm_network_security_group.nsg_apim.id
  role_definition_name = "Network Contributor"
  principal_id         = "42aa6557-76a3-430f-b9ff-ddb88c4f99a8"
}
