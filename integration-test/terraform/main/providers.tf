terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider
    azurerm = "3.73.0"

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
  host      = "https://${azurerm_databricks_workspace.this.workspace_url}"
  token     = data.external.databricks_token_integration_test.result.pat_token
}
