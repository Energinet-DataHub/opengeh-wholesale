resource "azurerm_subnet" "inbounddns" {
  name                 = "snet-inbounddns"
  resource_group_name  = var.virtual_network_resource_group_name
  virtual_network_name = data.azurerm_virtual_network.this.name
  address_prefixes     = var.inbounddns_address_prefixes

  delegation {
    name = "Microsoft.Network.dnsResolvers"
    service_delegation {
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
      name    = "Microsoft.Network/dnsResolvers"
    }
  }
}

resource "azurerm_subnet" "outbounddns" {
  name                 = "snet-outbounddns"
  resource_group_name  = var.virtual_network_resource_group_name
  virtual_network_name = data.azurerm_virtual_network.this.name
  address_prefixes     = var.outbounddns_address_prefixes

  delegation {
    name = "Microsoft.Network.dnsResolvers"
    service_delegation {
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
      name    = "Microsoft.Network/dnsResolvers"
    }
  }
}

resource "azurerm_private_dns_resolver" "this" {
  name                = "dnspr-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = data.azurerm_virtual_network.this.location
  virtual_network_id  = data.azurerm_virtual_network.this.id

  tags = local.tags
}

resource "azurerm_private_dns_resolver_inbound_endpoint" "inbounddns" {
  name                    = "in-${local.resources_suffix}"
  private_dns_resolver_id = azurerm_private_dns_resolver.this.id
  location                = azurerm_private_dns_resolver.this.location
  ip_configurations {
    private_ip_allocation_method = "Dynamic"
    subnet_id                    = azurerm_subnet.inbounddns.id
  }

  tags = local.tags
}

resource "azurerm_private_dns_resolver_outbound_endpoint" "outbounddns" {
  name                    = "out-${local.resources_suffix}"
  private_dns_resolver_id = azurerm_private_dns_resolver.this.id
  location                = azurerm_private_dns_resolver.this.location
  subnet_id               = azurerm_subnet.outbounddns.id

  tags = local.tags
}

resource "azurerm_private_dns_resolver_dns_forwarding_ruleset" "outbounddns" {
  name                                       = "dnsfrs-${local.resources_suffix}"
  resource_group_name                        = azurerm_resource_group.this.name
  location                                   = azurerm_resource_group.this.location
  private_dns_resolver_outbound_endpoint_ids = [azurerm_private_dns_resolver_outbound_endpoint.outbounddns.id]

  tags = local.tags
}

resource "azurerm_private_dns_resolver_forwarding_rule" "cp_dns" {
  for_each = tomap({
    blob        = "privatelink.blob.core.windows.net."
    dfs         = "privatelink.dfs.core.windows.net."
    queue       = "privatelink.queue.core.windows.net."
    database    = "privatelink.database.windows.net."
    servicebus  = "privatelink.servicebus.windows.net."
  })

  name                      = "fr-${each.key}"
  dns_forwarding_ruleset_id = azurerm_private_dns_resolver_dns_forwarding_ruleset.outbounddns.id
  domain_name               = each.value
  enabled                   = true
  target_dns_servers {
    ip_address = var.udr_firewall_next_hop_ip_address # Firewall acts as DNS
    port       = 53
  }
}

module "dns_resolver_forwarding_ruleset_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "dns-resolver-forwarding-ruleset-id"
  value        = azurerm_private_dns_resolver_dns_forwarding_ruleset.outbounddns.id
  key_vault_id = module.kv_shared.id
}
