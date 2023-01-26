terraform {
  backend "azurerm" {
    resource_group_name   = "@resource_group_name"
    storage_account_name  = "@storage_account_name"
    container_name        = "tfstate"
    key                   = "terraform_infra.tfstate"
    use_oidc              = true
    subscription_id       = "@azure_subscription_id"
    tenant_id             = "@azure_tenant_id"
}
}
