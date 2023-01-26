resource "azurerm_api_management_backend" "market_roles" {
  name                = "market_roles"
  resource_group_name = azurerm_resource_group.this.name
  api_management_name = module.apim_shared.name
  protocol            = "http"
  url                 = var.apimao_market_roles_domain_ingestion_function_url  
}