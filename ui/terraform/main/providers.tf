terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.6.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "3.0.2"
    }
  }
}

provider "azurerm" {
  use_oidc            = true
  storage_use_azuread = true
  features {}
}

provider "azuread" {
  use_oidc = true
}

provider "azuread" {
  alias     = "b2c"
  use_oidc  = true
  tenant_id = var.b2c_tenant_id
  client_id = var.b2c_client_id
}
