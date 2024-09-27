terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider
    azurerm = "3.113.0"

    azuread = "2.47.0"

    azapi = {
      source  = "Azure/azapi"
      version = "1.12.1"
    }
  }
}

provider "azurerm" {
  use_oidc            = true
  storage_use_azuread = true
  features {}
}

provider "azapi" {
  use_oidc = true
}
