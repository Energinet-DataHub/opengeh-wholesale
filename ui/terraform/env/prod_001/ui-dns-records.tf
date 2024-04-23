resource "azurerm_dns_cname_record" "www" {
  name                = "www"
  zone_name           = var.frontend_url
  resource_group_name = data.azurerm_resource_group.shared.name
  ttl                 = 3600
  record              = azurerm_static_site.ui.default_host_name
}

# Add wait time to allow for the CNAME record to be propagated
resource "time_sleep" "wait_60_seconds" {
  create_duration = "60s"

  depends_on = [azurerm_dns_cname_record.www]
}

# Allow for www.datahub3.dk - needs to depend on the CNAME record for it to be validated
resource "azurerm_static_site_custom_domain" "www" {
  static_site_id  = azurerm_static_site.ui.id
  domain_name     = "www.${local.frontend_url}"
  validation_type = "cname-delegation"

  depends_on = [azurerm_dns_cname_record.www, time_sleep.wait_60_seconds]
}
