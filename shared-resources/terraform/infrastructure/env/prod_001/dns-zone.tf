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

# APIM B2B dev_001
resource "azurerm_dns_cname_record" "dev_b2b" {
  name                = "dev.b2b"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "apim-shres-d-we-001.azure-api.net"
}

# APIM ebix dev_001
resource "azurerm_dns_cname_record" "dev_ebix" {
  name                = "dev.ebix"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "apim-shres-d-we-001.azure-api.net"
}

# APIM B2B test_001
resource "azurerm_dns_cname_record" "test_b2b" {
  name                = "test.b2b"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "apim-shres-t-we-001.azure-api.net"
}

# APIM ebix test_001
resource "azurerm_dns_cname_record" "test_ebix" {
  name                = "test.ebix"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "apim-shres-t-we-001.azure-api.net"
}

# APIM B2B preprod_001
resource "azurerm_dns_cname_record" "preprod_b2b" {
  name                = "preprod.b2b"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "apim-shres-b-we-001.azure-api.net"
}

# APIM ebix preprod_001
resource "azurerm_dns_cname_record" "preprod_ebix" {
  name                = "preprod.ebix"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "apim-shres-b-we-001.azure-api.net"
}

# APIM B2B prod_001
resource "azurerm_dns_cname_record" "prod_b2b" {
  name                = "b2b"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "apim-shres-p-we-001.azure-api.net"
}

# APIM ebix prod_001
resource "azurerm_dns_cname_record" "prod_ebix" {
  name                = "ebix"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "apim-shres-p-we-001.azure-api.net"
}

# UI dev_001
resource "azurerm_dns_cname_record" "dev001_ui" {
  name                = "dev"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "nice-meadow-03a161503.3.azurestaticapps.net"
}


# UI test_001
resource "azurerm_dns_cname_record" "test001_ui" {
  name                = "test"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "green-beach-024d44703.3.azurestaticapps.net"
}

# UI preprod_001
resource "azurerm_dns_cname_record" "preprod_ui" {
  name                = "preprod"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "red-meadow-02d1f9403.3.azurestaticapps.net"
}

# sauron prod_001
resource "azurerm_dns_cname_record" "prod_sauron" {
  name                = "sauron"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "jolly-field-0d52edd03.4.azurestaticapps.net"
}
