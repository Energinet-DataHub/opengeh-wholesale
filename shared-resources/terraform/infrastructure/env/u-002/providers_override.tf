terraform {
  required_providers {
    azuread = {
      source  = "hashicorp/azuread"
      version = "2.31.0"
    }
  }
}

provider "azuread" {
  use_oidc = true
}
