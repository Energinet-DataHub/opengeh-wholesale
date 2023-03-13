terraform {
  required_providers {
    azuread = {
      source  = "hashicorp/azuread"
      version = "2.36.0"
    }
  }
}

provider "azuread" {
  tenant_id     = var.b2c_tenant_id
  client_id     = var.b2c_client_id
  client_secret = var.b2c_client_secret
}
