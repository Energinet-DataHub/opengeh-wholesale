resource "azurerm_static_site_custom_domain" "this" {
  static_site_id  = azurerm_static_site.ui.id
  domain_name     = local.frontend_url
  validation_type = "cname-delegation"
}
