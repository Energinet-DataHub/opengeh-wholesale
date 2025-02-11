terraform {
  required_providers {
    databricks = {
      source  = "databricks/databricks"
      version = "1.64.1"
    }
    # It is recommended to pin to a given version of the Azure provider.
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.17.0"
    }

    shell = {
      source  = "scottwinkler/shell"
      version = "1.7.10"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "3.1.0"
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
