terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider
    # We cannot bump this to 3.85.0 currently, as Terraform has added an enhancement in 3.84.0 that states a preview feature must be enabled on SQL databases and elastic pools
    # See https://learn.microsoft.com/en-us/azure/azure-sql/database/always-encrypted-enclaves-enable?view=azuresql&tabs=VBSenclaves
    azurerm = "3.83.0"

    azuread = "2.39.0"
  }
}

provider "azurerm" {
  use_oidc            = true
  storage_use_azuread = true
  features {}
}
