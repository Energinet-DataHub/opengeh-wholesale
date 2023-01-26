terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider
    azurerm = "=3.9.0"

    databricks = {
      source = "databricks/databricks"
      version = "1.5.0"
    }

    azuread = {
      source = "hashicorp/azuread"
      version = "2.31.0"
    }
  }
}

provider "databricks" {
  auth_type = "pat"
  host      = "https://${data.azurerm_key_vault_secret.dbw_databricks_workspace_url.value}"
  token     = data.azurerm_key_vault_secret.dbw_databricks_workspace_token.value
}

provider "azurerm" {
  use_oidc = true
  features {}
}

provider "azuread" {
  use_oidc = true
}