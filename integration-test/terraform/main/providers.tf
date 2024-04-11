terraform {
  required_providers {
    azurerm = "3.94.0"

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
  auth_type = "pat"
  host      = "https://${azurerm_databricks_workspace.this.workspace_url}"
  token     = data.external.databricks_token_integration_test.result.pat_token
}
