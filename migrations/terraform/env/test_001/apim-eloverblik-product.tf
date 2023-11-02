resource "azurerm_api_management_product" "apim_product_eloverblik" {
  api_management_name   = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name   = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  product_id            = "eloverblik-product"
  display_name          = "Eloverblik Product"
  subscription_required = true
  approval_required     = false
  published             = true
}

resource "azurerm_api_management_product_api" "apim_product_eloverblik_timeseriesapi" {
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_name            = module.apim_timeseriesapi.name
  product_id          = azurerm_api_management_product.apim_product_eloverblik.product_id
}

resource "azurerm_api_management_group" "apim_group_eloverblik" {
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  name                = "eloverblik-developer-group"
  display_name        = "Eloverblik developers"
  description         = "Group for developers of Eloverblik that needs access to the API documentation"
}

resource "azurerm_api_management_product_group" "apim_product_eloverblik_group" {
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  product_id          = azurerm_api_management_product.apim_product_eloverblik.product_id
  group_name          = azurerm_api_management_group.apim_group_eloverblik.name
}
