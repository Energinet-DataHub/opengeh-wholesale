terraform {
  required_providers {
    azuread = "2.36.0"
  }
}

provider "azuread" {
  use_oidc  = true
  tenant_id = var.b2c_tenant_id
  client_id = var.b2c_client_id
}
