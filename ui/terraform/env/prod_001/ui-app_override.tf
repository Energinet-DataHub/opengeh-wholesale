resource "azurerm_static_site_custom_domain" "this" {
  validation_type = "dns-txt-token"
}
