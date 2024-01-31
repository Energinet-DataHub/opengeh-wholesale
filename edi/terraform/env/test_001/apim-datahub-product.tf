resource "azurerm_api_management_product" "apim_product_datahub" {
  api_management_name   = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name   = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  product_id            = "datahub-product"
  display_name          = "Datahub Product"
  subscription_required = true
  approval_required     = false
  published             = true
}

resource "azurerm_api_management_product_api" "apim_product_datahub_edi_api" {
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_name            = module.apima_b2b.name
  product_id          = azurerm_api_management_product.apim_product_datahub.product_id
}

resource "azurerm_api_management_group" "apim_group_datahub" {
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  name                = "datahub-developer-group"
  display_name        = "Datahub developers"
  description         = "Group for developers of Datahub that needs access to the API documentation"
}

resource "azurerm_api_management_product_group" "apim_product_datahub_group" {
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  product_id          = azurerm_api_management_product.apim_product_datahub.product_id
  group_name          = azurerm_api_management_group.apim_group_datahub.name
}
