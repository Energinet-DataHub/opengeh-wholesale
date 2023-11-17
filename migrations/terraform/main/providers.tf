terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider
    azurerm = "=3.73.0"

    databricks = {
      source  = "databricks/databricks"
      version = "1.13.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "2.36.0"
    }
  }
}

provider "databricks" {
  auth_type = "pat"
  host      = "https://${data.azurerm_key_vault_secret.dbw_databricks_workspace_url.value}"
  token     = data.azurerm_key_vault_secret.dbw_databricks_workspace_token.value
}

provider "databricks" {
  alias     = "dbw"
  auth_type = "pat"
  host      = "https://${module.dbw.workspace_url}"
  token     = module.dbw.databricks_token
}

provider "azurerm" {
  use_oidc            = true
  storage_use_azuread = true
  features {}
}

provider "azuread" {
  use_oidc = true
}
