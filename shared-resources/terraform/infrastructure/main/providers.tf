terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider
    azurerm = "3.79.0"

    azuread = "2.39.0"

    databricks = {
      source  = "databricks/databricks"
      version = "1.13.0"
    }
  }
}

provider "azurerm" {
  use_oidc            = true
  storage_use_azuread = true
  features {}
}

provider "databricks" {
  auth_type = "pat"
  host      = "https://${module.dbw_shared.workspace_url}"
  token     = module.dbw_shared.databricks_token
}
