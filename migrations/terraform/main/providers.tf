terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider
    azurerm = "3.97.1"

    databricks = {
      source  = "databricks/databricks"
      version = "1.38.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "2.47.0"
    }

    azapi = {
      source  = "Azure/azapi"
      version = "1.12.1"
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

provider "azuread" {
  use_oidc = true
}

provider "azapi" {
  use_oidc = true
}
