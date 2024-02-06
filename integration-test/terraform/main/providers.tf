terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider
    # We cannot bump this to 3.85.0 currently, as Terraform has added an enhancement in 3.84.0 that states a preview feature must be enabled on SQL databases and elastic pools
    # See https://learn.microsoft.com/en-us/azure/azure-sql/database/always-encrypted-enclaves-enable?view=azuresql&tabs=VBSenclaves
    azurerm = "3.83.0"

    databricks = {
      source  = "databricks/databricks"
      version = "1.31.1"
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
