terraform {
  required_providers {
    azuread = {
      source  = "hashicorp/azuread"
      version = "2.47.0"
    }
  }
}

provider "azuread" {
  use_oidc  = true
  tenant_id = var.b2c_tenant_id
  client_id = var.b2c_client_id
}
