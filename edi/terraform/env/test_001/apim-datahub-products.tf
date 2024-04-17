resource "azurerm_api_management_product" "apim_product_datahub" {
  api_management_name   = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name   = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  product_id            = "datahub-product"
  display_name          = "Datahub Product"
  subscription_required = true
  approval_required     = false
  published             = true
}

# Built-in 'Guests' group
data "azurerm_api_management_group" "guests" {
  name                = "Guests"
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
}

### <Datahub product> ###
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

# Add access for 'Guests'
resource "azurerm_api_management_product_group" "apim_product_datahub_group_guests" {
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  product_id          = azurerm_api_management_product.apim_product_datahub.product_id
  group_name          = lower(data.azurerm_api_management_group.guests.name)  # See https://github.com/hashicorp/terraform-provider-azurerm/issues/17619#issuecomment-1403127161
}
### </ Datahub product> ###


### <Datahub EBIX product> ###
resource "azurerm_api_management_product" "apim_product_datahub_ebix" {
  api_management_name   = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name   = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  product_id            = "datahub-ebix-product"
  display_name          = "Datahub Ebix Product"
  subscription_required = true
  approval_required     = false
  published             = true
}

resource "azurerm_api_management_product_api" "apim_product_datahub_edi_ebix_api" {
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_name            = module.apima_b2b_ebix.name
  product_id          = azurerm_api_management_product.apim_product_datahub_ebix.product_id
}

resource "azurerm_api_management_group" "apim_group_datahub_ebix" {
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  name                = "datahub-ebix-developer-group"
  display_name        = "Datahub Ebix developers"
  description         = "Group for developers of Datahub that needs access to the Ebix API documentation"
}

resource "azurerm_api_management_product_group" "apim_product_datahub_group_ebix" {
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  product_id          = azurerm_api_management_product.apim_product_datahub_ebix.product_id
  group_name          = azurerm_api_management_group.apim_group_datahub_ebix.name
}

# Add access for 'Guests'
resource "azurerm_api_management_product_group" "apim_product_datahub_group_ebix_guests" {
  api_management_name = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  product_id          = azurerm_api_management_product.apim_product_datahub_ebix.product_id
  group_name          = lower(data.azurerm_api_management_group.guests.name) # See https://github.com/hashicorp/terraform-provider-azurerm/issues/17619#issuecomment-1403127161
}

### </ Datahub EBIX product> ###
