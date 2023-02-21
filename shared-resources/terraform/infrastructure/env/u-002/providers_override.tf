terraform {
  required_providers {
    azurerm = "=3.37.0"
    databricks = {
      source = "databricks/databricks"
      version = "1.9.1"
    }

    azuread = {
      source = "hashicorp/azuread"
      version = "2.31.0"
    }
  }
}

provider "databricks" {
  auth_type = "pat"
  host      = "https://${azurerm_databricks_workspace.playground.workspace_url}"
  token     = data.external.databricks_token_playground.result.pat_token
}

provider "azuread" {
  use_oidc = true
}