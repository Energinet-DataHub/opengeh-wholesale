locals {
  deployagent_count = 3
}

resource "azurerm_linux_virtual_machine" "deployagent" {
    size                            = "Standard_DS5_v2"
}