locals {
  # DO NOT ALTER THE ORDER OF THE ENVIRONMENTS, ONLY APPEND!
  frontend_urls = [
    {
      environment = "dev002"
      url         = "lively-pond-06cbcb903.3.azurestaticapps.net"
    },
    {
      environment = "test002"
      url         = "agreeable-mud-0a0a24403.3.azurestaticapps.net"
    },
    {
      environment = "sauron.test002"
      url         = "victorious-ocean-05a6eaa03.5.azurestaticapps.net"
    },
    {
      environment = "dev"
      url         = "nice-meadow-03a161503.3.azurestaticapps.net"
    },
    {
      environment = "test"
      url         = "green-beach-024d44703.3.azurestaticapps.net"
    },
    {
      environment = "preprod"
      url         = "red-meadow-02d1f9403.3.azurestaticapps.net"
    },
    {
      environment = "prod"
      url         = "calm-sky-050b30c03.3.azurestaticapps.net"
    },
    {
      environment = "sauron"
      url         = "jolly-field-0d52edd03.4.azurestaticapps.net"
    },
    {
      environment = "www"
      url         = "calm-sky-050b30c03.3.azurestaticapps.net"
    }
  ]
}

# Create an origin group per environment for UI
resource "azurerm_cdn_frontdoor_origin_group" "ui" {
  count                    = length(local.frontend_urls)
  name                     = replace("${local.frontend_urls[count.index].environment}-ui", ".", "-")
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.this.id

  load_balancing {
    additional_latency_in_milliseconds = 0
    sample_size                        = 4
    successful_samples_required        = 3
  }
}

# Create an origin per environment for UI
resource "azurerm_cdn_frontdoor_origin" "ui" {
  count                         = length(local.frontend_urls)
  name                          = replace("${local.frontend_urls[count.index].environment}-ui", ".", "-")
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.ui[count.index].id
  enabled                       = true

  certificate_name_check_enabled = true

  host_name          = local.frontend_urls[count.index].url
  http_port          = 80
  https_port         = 443
  origin_host_header = local.frontend_urls[count.index].url
}

# Create an endpoint per static web app
resource "azurerm_cdn_frontdoor_endpoint" "ui" {
  count                    = length(local.frontend_urls)
  name                     = replace("ui-${local.frontend_urls[count.index].environment}", ".", "-")
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.this.id
}

# Create Front Door custom domains
resource "azurerm_cdn_frontdoor_custom_domain" "ui" {
  count                    = length(local.frontend_urls)
  name                     = replace("${local.frontend_urls[count.index].environment}-ui", ".", "-")
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.this.id
  dns_zone_id              = azurerm_dns_zone.this.id
  # If prod, use the root domain, otherwise use the subdomain
  host_name = (local.frontend_urls[count.index].environment == "prod" ? azurerm_dns_zone.this.name : "${local.frontend_urls[count.index].environment}.${azurerm_dns_zone.this.name}")

  tls {
    certificate_type    = "ManagedCertificate"
    minimum_tls_version = "TLS12"
  }
}

# Create the CNAME records for the Front Door in Azure DNS
resource "azurerm_dns_cname_record" "ui_frontdoor" {
  count               = length(local.frontend_urls)
  name                = local.frontend_urls[count.index].environment
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  record              = azurerm_cdn_frontdoor_endpoint.ui[count.index].host_name
}

# Create the TXT records for the Front Door in Azure DNS - needed to validate ownership of the DNS zone
resource "azurerm_dns_txt_record" "ui_frontdoor" {
  count               = length(local.frontend_urls)
  name                = (local.frontend_urls[count.index].environment == "prod" ? "_dnsauth" : "_dnsauth.${local.frontend_urls[count.index].environment}")
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600

  record {
    value = azurerm_cdn_frontdoor_custom_domain.ui[count.index].validation_token
  }
}

# Create the Front Door routes, each per environment
resource "azurerm_cdn_frontdoor_route" "ui" {
  count                         = length(local.frontend_urls)
  name                          = replace("${local.frontend_urls[count.index].environment}-ui", ".", "-")
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.ui[count.index].id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.ui[count.index].id
  cdn_frontdoor_origin_ids      = [azurerm_cdn_frontdoor_origin.ui[count.index].id]
  enabled                       = true

  forwarding_protocol    = "HttpsOnly"
  https_redirect_enabled = true
  patterns_to_match      = ["/*"]
  supported_protocols    = ["Http", "Https"]

  link_to_default_domain = true

  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.ui[count.index].id]
}

# Associate the custom domains with the routes
resource "azurerm_cdn_frontdoor_custom_domain_association" "ui" {
  count                          = length(local.frontend_urls)
  cdn_frontdoor_custom_domain_id = azurerm_cdn_frontdoor_custom_domain.ui[count.index].id
  cdn_frontdoor_route_ids        = [azurerm_cdn_frontdoor_route.ui[count.index].id]
  depends_on                     = [azurerm_cdn_frontdoor_route.ui]
}

# Add A record for prod to target the Front Door endpoint
resource "azurerm_dns_a_record" "prod" {
  name                = "@"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_resource_group.this.name
  ttl                 = 3600
  # Get the target endpoint for the prod environment by finding index where environment is prod
  target_resource_id = azurerm_cdn_frontdoor_endpoint.ui[index(local.frontend_urls.*.environment, "prod")].id
}
