terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.17.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "3.1.0"
    }
    # We cannot update higher currently, as the new version has a big the cluster where it says we use singlenode, but we use multinode
    databricks = {
      source  = "databricks/databricks"
      version = "1.65.0"
    }
  }
}

provider "azurerm" {
  use_oidc            = true
  storage_use_azuread = true
  features {}
}

provider "azuread" {
  use_oidc = true
}

provider "azuread" {
  # Target the B2C tenant
  alias     = "b2c"
  use_oidc  = true
  tenant_id = var.b2c_tenant_id
  client_id = var.b2c_client_id
}

provider "databricks" {
  host = "https://${azurerm_databricks_workspace.this.workspace_url}"
}

