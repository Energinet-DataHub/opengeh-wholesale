terraform {
  required_providers {
    databricks = {
      source  = "databricks/databricks"
      version = "1.49.1"
    }
    # It is recommended to pin to a given version of the Azure provider
    azurerm = "3.113.0"

    shell = {
      source  = "scottwinkler/shell"
      version = "1.7.10"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "2.47.0"
    }
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
