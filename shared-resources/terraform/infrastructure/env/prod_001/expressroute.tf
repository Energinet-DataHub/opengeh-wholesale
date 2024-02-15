resource "azurerm_express_route_circuit" "dh2_express_route" {
  name                  = "erc-${local.resources_suffix}"
  resource_group_name   = azurerm_resource_group.this.name
  location              = azurerm_resource_group.this.location
  service_provider_name = "Interxion"
  peering_location      = "Copenhagen"
  bandwidth_in_mbps     = 1000

  sku {
    tier   = "Standard"
    family = "MeteredData"
  }

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_route_filter" "this" {
  name                = "rf-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

  rule {
    name        = "allow-rule"
    access      = "Allow"
    rule_type   = "Community"
    communities = ["12076:52002"]
  }

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_express_route_circuit_peering" "this" {
  peering_type                  = "MicrosoftPeering"
  express_route_circuit_name    = azurerm_express_route_circuit.dh2_express_route.name
  resource_group_name           = azurerm_resource_group.this.name
  peer_asn                      = 47292
  primary_peer_address_prefix   = "86.106.96.248/30"
  secondary_peer_address_prefix = "86.106.96.252/30"
  ipv4_enabled                  = true
  vlan_id                       = 11
  shared_key                    = var.shared_key_cgi
  route_filter_id               = azurerm_route_filter.this.id

  microsoft_peering_config {
    advertised_public_prefixes = ["86.106.96.1/32", "86.106.96.2/32"]
    advertised_communities     = ["12076:52002"]
  }

  lifecycle {
    prevent_destroy = true
  }
}

# ReadOnly means authorized users can only read from a resource, but they can't modify or delete it.
resource "azurerm_management_lock" "erc-lock" {
  name       = "erc-lock"
  scope      = azurerm_express_route_circuit.dh2_express_route.id
  lock_level = "ReadOnly"
  notes      = "Locked as the Express Route is needed for CGI"
}
