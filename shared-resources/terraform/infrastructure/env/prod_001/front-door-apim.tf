locals {
  environment_short = ["d-002", "t-002", "d-001", "t-001", "b-001", "p-001"]     # Only append to the list due to indexes!
  environments      = ["dev002", "test002", "dev", "test", "preprod", "prod001"] # Only append to the list due to indexes! prod001 is the only one with a number, as it is checked later on
  type              = ["ebix", "b2b"]                                            # Only append to the list due to indexes!
  cartesian         = setproduct(local.environments, local.type)
}

# Create an origin group per environment for APIM
resource "azurerm_cdn_frontdoor_origin_group" "this" {
  count                    = length(local.environments)
  name                     = "${local.environments[count.index]}-apim"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.this.id

  load_balancing {
    additional_latency_in_milliseconds = 0
    sample_size                        = 4
    successful_samples_required        = 3
  }
}

# Create an origin per environment for API Management
resource "azurerm_cdn_frontdoor_origin" "this" {
  count                         = length(local.environment_short)
  name                          = local.environment_short[count.index]
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.this[count.index].id
  enabled                       = true

  certificate_name_check_enabled = true

  # Only get the first part of the envinronment name and the instance
  host_name          = "apim-shres-${substr(local.environment_short[count.index], 0, 1)}-we-${substr(local.environment_short[count.index], 2, 5)}.azure-api.net"
  http_port          = 80
  https_port         = 443
  origin_host_header = "apim-shres-${substr(local.environment_short[count.index], 0, 1)}-we-${substr(local.environment_short[count.index], 2, 5)}.azure-api.net"
}

# Create an endpoint per environment
resource "azurerm_cdn_frontdoor_endpoint" "this" {
  count                    = length(local.environments)
  name                     = local.environments[count.index]
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.this.id
}

# Create Front Door custom domains
resource "azurerm_cdn_frontdoor_custom_domain" "this" {
  count                    = length(local.cartesian)
  name                     = "${local.cartesian[count.index][0]}-${local.cartesian[count.index][1]}"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.this.id
  dns_zone_id              = azurerm_dns_zone.this.id
  # If prod, use the root domain, otherwise use the subdomain
  host_name = (local.cartesian[count.index][0] == "prod001" ? "${local.cartesian[count.index][1]}.${azurerm_dns_zone.this.name}" : "${local.cartesian[count.index][0]}.${local.cartesian[count.index][1]}.${azurerm_dns_zone.this.name}")

  tls {
    certificate_type    = "ManagedCertificate"
    minimum_tls_version = "TLS12"
  }
}

# Create the CNAME records for the Front Door in Azure DNS
resource "azurerm_dns_cname_record" "frontdoor" {
  count               = length(local.cartesian)
  name                = (local.cartesian[count.index][0] == "prod001" ? local.cartesian[count.index][1] : "${local.cartesian[count.index][0]}.${local.cartesian[count.index][1]}")
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = azurerm_cdn_frontdoor_endpoint.this[index(local.environments, local.cartesian[count.index][0])].host_name
}

# Create the TXT records for the Front Door in Azure DNS - needed to validate ownership of the DNS zone
resource "azurerm_dns_txt_record" "frontdoor" {
  count               = length(local.cartesian)
  name                = (local.cartesian[count.index][0] == "prod001" ? "_dnsauth.${local.cartesian[count.index][1]}" : "_dnsauth.${local.cartesian[count.index][0]}.${local.cartesian[count.index][1]}")
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600

  record {
    value = azurerm_cdn_frontdoor_custom_domain.this[count.index].validation_token
  }
}

# Create the Front Door routes, each per environment
resource "azurerm_cdn_frontdoor_route" "this" {
  count                         = length(local.environments)
  name                          = local.environments[count.index]
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.this[count.index].id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.this[count.index].id
  cdn_frontdoor_origin_ids      = [azurerm_cdn_frontdoor_origin.this[count.index].id]
  enabled                       = true

  forwarding_protocol    = "HttpsOnly"
  https_redirect_enabled = true
  patterns_to_match      = ["/*"]
  supported_protocols    = ["Http", "Https"]

  link_to_default_domain = true

  # Get all where the first part of the environment is the same as the current environment
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.this[index(local.cartesian, [local.environments[count.index], "ebix"])].id, azurerm_cdn_frontdoor_custom_domain.this[index(local.cartesian, [local.environments[count.index], "b2b"])].id]
}

# Associate the custom domains with the routes
resource "azurerm_cdn_frontdoor_custom_domain_association" "this" {
  count                          = length(local.cartesian)
  cdn_frontdoor_custom_domain_id = azurerm_cdn_frontdoor_custom_domain.this[count.index].id
  # Both the ebix and b2b uses the same route, therefore we cant use the count.index, we need to use the index of the environment
  cdn_frontdoor_route_ids = [azurerm_cdn_frontdoor_route.this[index(local.environments, local.cartesian[count.index][0])].id]
  depends_on              = [azurerm_cdn_frontdoor_route.this]
}
