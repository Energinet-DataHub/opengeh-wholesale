resource "azurerm_dns_zone" "this" {
  name                = local.frontend_url
  resource_group_name = azurerm_resource_group.this.name
}

resource "azurerm_dns_txt_record" "this" {
  name                = "@"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record {
    value = var.txt_value
  }
}
