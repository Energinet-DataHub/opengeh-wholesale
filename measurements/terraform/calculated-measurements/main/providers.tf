terraform {
  required_providers {
    databricks = {
      source  = "databricks/databricks"
      version = "1.64.1"
    }
    # It is recommended to pin to a given version of the Azure provider
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.17.0"
    }

    shell = {
      source  = "scottwinkler/shell"
      version = "1.7.10"
    }
  }
}

provider "databricks" {
  alias = "dbw"
  host  = "https://${data.azurerm_key_vault_secret.mmcore_databricks_workspace_url.value}"
}

provider "azurerm" {
  use_oidc            = true
  storage_use_azuread = true
  features {}
}
