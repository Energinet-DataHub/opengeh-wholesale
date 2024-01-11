# The DNS zone must only be created once and it is therefore created in prod_001.

resource "azurerm_dns_zone" "this" {
  name                = "datahub3.dk"
  resource_group_name = azurerm_resource_group.this.name
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

# APIM B2B dev_002
resource "azurerm_dns_cname_record" "dev002_b2b" {
  name                = "dev002.b2b"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "apim-shres-d-we-002.azure-api.net"
}

# APIM B2B dev_002
resource "azurerm_dns_cname_record" "dev002_ebix" {
  name                = "dev002.ebix"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "apim-shres-d-we-002.azure-api.net"
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

# APIM B2B test_002
resource "azurerm_dns_cname_record" "test002_b2b" {
  name                = "test002.b2b"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "apim-shres-t-we-002.azure-api.net"
}

# APIM ebix test_002
resource "azurerm_dns_cname_record" "test002_ebix" {
  name                = "test002.ebix"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "apim-shres-t-we-002.azure-api.net"
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

# UI dev_002
resource "azurerm_dns_cname_record" "dev002_ui" {
  name                = "dev002"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "lively-pond-06cbcb903.3.azurestaticapps.net"
}

# UI test_001
resource "azurerm_dns_cname_record" "test001_ui" {
  name                = "test"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "green-beach-024d44703.3.azurestaticapps.net"
}

# UI test_002
resource "azurerm_dns_cname_record" "test002_ui" {
  name                = "test002"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "agreeable-mud-0a0a24403.3.azurestaticapps.net"
}

# UI sandbox_002
resource "azurerm_dns_cname_record" "sandbox002_ui" {
  name                = "sandbox"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "happy-desert-06b3ac903.4.azurestaticapps.net"
}

# UI preprod_001
resource "azurerm_dns_cname_record" "preprod_ui" {
  name                = "preprod"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "red-meadow-02d1f9403.3.azurestaticapps.net"
}

# UI prod_001
resource "azurerm_dns_cname_record" "prod_ui" {
  name                = "www"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "calm-sky-050b30c03.3.azurestaticapps.net"
}

# sauron test_002
resource "azurerm_dns_cname_record" "test002_sauron" {
  name                = "sauron.test002"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "black-sky-0fe28c503.4.azurestaticapps.net"
}


# sauron prod_001
resource "azurerm_dns_cname_record" "prod_sauron" {
  name                = "sauron"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = "jolly-field-0d52edd03.4.azurestaticapps.net"
}
