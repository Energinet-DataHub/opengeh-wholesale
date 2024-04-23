# The DNS zone must only be created once and it is therefore created in prod_001.

resource "azurerm_dns_zone" "this" {
  name                = "datahub3.dk"
  resource_group_name = azurerm_resource_group.this.name

  lifecycle {
    prevent_destroy = true
  }
}

# If deleted the new DNS zone URLs will have to be changed with IT operations!
resource "azurerm_management_lock" "dns-zone-lock" {
  name       = "dns-zone-lock"
  scope      = azurerm_dns_zone.this.id
  lock_level = "CanNotDelete"
  notes      = "Locked as the DNS zones must be static"
}

# Verification code to verify ownership of the domain
resource "azurerm_dns_txt_record" "this" {
  name                = "@"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600

  record {
    value = var.domain_verification_code
  }
}
