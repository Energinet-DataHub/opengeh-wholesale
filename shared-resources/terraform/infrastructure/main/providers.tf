terraform {
  required_providers {
    # It is recommended to pin to a given version of the Azure provider

    # --> DO NOT UPGRADE to >= 3.93.0 before this issue has been fixed: https://github.com/hashicorp/terraform-provider-azurerm/issues/25273
    azurerm = "3.92.0" # See also https://github.com/Energinet-DataHub/geh-terraform-modules/pull/310 for details
    azuread = "2.47.0"
  }
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
