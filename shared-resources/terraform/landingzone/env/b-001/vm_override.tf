locals {
  deployagent_count = 2
}

resource "azurerm_linux_virtual_machine" "deployagent" {
    size                            = "Standard_DS5_v2"
}