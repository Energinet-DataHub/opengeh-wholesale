terraform {
  required_providers {
    azurerm = "3.99.0"

    databricks = {
      source  = "databricks/databricks"
      version = "1.38.0"
    }
  }
}

provider "azurerm" {
  use_oidc            = true
  storage_use_azuread = true
  features {}
}

provider "databricks" {
  host = "https://${azurerm_databricks_workspace.this.workspace_url}"
}
