resource "azurerm_static_site_custom_domain" "this" {
  static_site_id = azurerm_static_site.ui.id
  domain_name    = "preprod.datahub3.dk"
  validation_type = "cname-delegation"
}
