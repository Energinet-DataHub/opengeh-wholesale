terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider

    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.6.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "3.0.2"
    }

    databricks = {
      source  = "databricks/databricks"
      version = "1.56.0"
    }

    shell = {
      source  = "scottwinkler/shell"
      version = "1.7.10"
    }

    grafana = {
      source  = "grafana/grafana"
      version = "3.10.0"
    }
  }
}

provider "grafana" {
  url  = azurerm_dashboard_grafana.this.endpoint
  auth = shell_script.create_key.output["key"]
}

provider "databricks" {
  alias = "dbw"
  host  = "https://${azurerm_databricks_workspace.this.workspace_url}"
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
