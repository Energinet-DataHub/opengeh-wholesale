resource "azurerm_static_site_custom_domain" "this" {
  validation_type = "cname-delegation"
}
