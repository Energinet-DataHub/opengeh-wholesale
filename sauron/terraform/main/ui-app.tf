resource "azurerm_static_site" "this" {
  name                = "stapp-ui-${local.name_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku_tier            = "Standard"
  sku_size            = "Standard"

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }

  tags = local.tags
}

resource "azurerm_static_site_custom_domain" "this" {
  static_site_id  = azurerm_static_site.this.id
  domain_name     = local.frontend_url
  validation_type = "cname-delegation"
}
