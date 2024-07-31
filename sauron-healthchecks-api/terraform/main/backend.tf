terraform {
  backend "azurerm" {
    resource_group_name  = "@resource_group_name"
    storage_account_name = "@storage_account_name"
    container_name       = "tfs"
    key                  = "sauron_healthchecks_api.tfs"
    use_oidc             = true
    use_azuread_auth     = true
    subscription_id      = "@azure_subscription_id"
    tenant_id            = "@azure_tenant_id"
  }
}
