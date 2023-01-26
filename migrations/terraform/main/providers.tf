terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider
    azurerm = "=3.9.0"

    azuread = {
      source = "hashicorp/azuread"
      version = "2.31.0"
    }
  }
}

provider "azurerm" {
  use_oidc = true
  features {}
}

provider "azuread" {
  use_oidc = true
}