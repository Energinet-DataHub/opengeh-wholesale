data "azurerm_network_security_group" "nsg_apim" {
  name                = "nsg-apim-shres"
  resource_group_name = data.azurerm_virtual_network.this.resource_group_name
}

resource "azurerm_chaos_studio_target" "target_apim" {
  location           = var.location
  target_resource_id = data.azurerm_network_security_group.nsg_apim.id
  target_type        = "Microsoft-NetworkSecurityGroup"
}

resource "azurerm_chaos_studio_capability" "nsg_apim_capability" {
  capability_type        = "SecurityRule-1.0"
  chaos_studio_target_id = azurerm_chaos_studio_target.target_apim.id
}

resource "azurerm_chaos_studio_experiment" "nsg_experiment_apim" {
  location            = var.location
  name                = "deny-app-services"
  resource_group_name = data.azurerm_virtual_network.this.resource_group_name

  identity {
    type = "SystemAssigned"
  }

  selectors {
    name                    = "ApimNsg"
    chaos_studio_target_ids = [azurerm_chaos_studio_target.target_apim.id]
  }

  steps {
    name = "test-nsg"
    branch {
      name = "deny-app-services"
      actions {
        urn           = azurerm_chaos_studio_capability.nsg_apim_capability.urn
        selector_name = "ApimNsg"
        parameters = {
          name                  = "chaos-experiment-rule-app-services"
          protocol              = "Tcp"
          sourceAddresses       = data.azurerm_subnet.snet_apim.address_prefixes[0]
          destinationAddresses  = data.azurerm_subnet.snet_private_endpoints.address_prefixes[0]
          action                = "Deny"
          destinationPortRanges = "[\"*\"]"
          sourcePortRanges      = "[\"*\"]"
          priority              = 1105
          direction             = "Outbound"
        }
        action_type = "continuous"
        duration    = "PT8H"
      }
    }
  }
}

resource "azurerm_role_assignment" "nsg_experiment_apims_network_contributor" {
  scope                = data.azurerm_network_security_group.nsg_apim.id
  role_definition_name = "Network Contributor"
  principal_id         = azurerm_chaos_studio_experiment.nsg_experiment_apim.identity[0].principal_id
}
