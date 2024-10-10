terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.3.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "2.47.0"
    }

    azapi = {
      source  = "Azure/azapi"
      version = "1.15.0"
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
