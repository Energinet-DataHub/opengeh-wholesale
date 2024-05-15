terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider

    azurerm = "3.97.1"
    azuread = "2.47.0"

    databricks = {
      source  = "databricks/databricks"
      version = "1.38.0"
    }
  }
}

provider "databricks" {
  alias = "dbw"
  host = "https://${azurerm_databricks_workspace.this.workspace_url}"
}

provider "azurerm" {
  use_oidc            = true
  storage_use_azuread = true
  features {
    api_management {
      purge_soft_delete_on_destroy = true
    }
  }
}
