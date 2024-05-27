terraform {
  required_providers {
    databricks = {
      source  = "databricks/databricks"
      version = "1.38.0"
    }
    # It is recommended to pin to a given version of the Azure provider
    azurerm = "3.98.0"
  }
}

provider "databricks" {
  alias = "dbw"
  host  = "https://${module.dbw.workspace_url}"
}

provider "azurerm" {
  use_oidc            = true
  storage_use_azuread = true
  features {}
}
