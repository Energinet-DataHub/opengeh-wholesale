terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider
    azurerm = "3.98.0"
  }
}

provider "azurerm" {
  use_oidc = true
  features {}
}
