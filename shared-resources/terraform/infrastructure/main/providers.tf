terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider
    azurerm = "=3.37.0"

    databricks = {
      source  = "databricks/databricks"
      version = "1.13.0"
    }
  }
}

provider "azurerm" {
  use_oidc = true
  features {}
}

provider "databricks" {
  auth_type = "pat"
  host      = "https://${module.dbw_shared.workspace_url}"
  token     = data.external.databricks_token.result.pat_token
}
