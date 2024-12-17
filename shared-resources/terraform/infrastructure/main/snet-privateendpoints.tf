resource "azurerm_subnet" "privateendpoints" {
  name                              = "snet-privateendpoints"
  resource_group_name               = var.virtual_network_resource_group_name
  virtual_network_name              = var.virtual_network_name
  address_prefixes                  = var.privateendpoints_address_prefixes
  private_endpoint_network_policies = "Enabled"
}

resource "azurerm_route_table" "privateendpoints" {
  name                = "rt-privateendpoints"
  location            = data.azurerm_virtual_network.this.location
  resource_group_name = var.virtual_network_resource_group_name

  route {
    name                   = "udr-azurefirewall"
    address_prefix         = "0.0.0.0/0"
    next_hop_type          = "VirtualAppliance"
    next_hop_in_ip_address = var.udr_firewall_next_hop_ip_address
  }

  tags = local.tags
}

resource "azurerm_subnet_route_table_association" "privateendpoints" {
  subnet_id      = azurerm_subnet.privateendpoints.id
  route_table_id = azurerm_route_table.privateendpoints.id
}

resource "azurerm_network_security_group" "privateendpoints" {
  name                = "nsg-privateendpoints"
  location            = data.azurerm_virtual_network.this.location
  resource_group_name = var.virtual_network_resource_group_name

  # INBOUND
  security_rule {
    name                       = "IBA-HTTPS-VNET"
    priority                   = 1000
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "VirtualNetwork"
  }
  security_rule {
    name                   = "IBA-HTTPS-VNET-LINK-LOCAL"
    priority               = 1001
    direction              = "Inbound"
    access                 = "Allow"
    protocol               = "Tcp"
    source_port_range      = "*"
    destination_port_range = "443"
    # add the private endpoints inbound to the Link Local Space / Void Space
    source_address_prefixes    = ["10.140.96.0/19", "10.142.96.0/19", "10.144.96.0/19", "10.146.96.0/19"]
    destination_address_prefix = "VirtualNetwork"
  }
  security_rule {
    name                       = "IBA-AllowSQLInbound"
    priority                   = 1010
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "1433"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "VirtualNetwork"
  }
  security_rule {
    name                       = "IBA-AllowAMQPInbound"
    priority                   = 1020
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "5671-5672"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "VirtualNetwork"
  }
  security_rule {
    name                       = "IBA-TrustedDevices"
    priority                   = 1030
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_ranges    = ["443", "1433", "5671-5672"]
    source_address_prefixes    = ["10.161.48.0/21", "10.185.0.0/16", "10.252.0.0/16", "10.200.62.0/23", "10.200.80.0/21", "10.140.41.0/24", "10.140.42.0/24"]
    destination_address_prefix = "VirtualNetwork"
  }
  dynamic "security_rule" {
    # P2S VPN is only on dev and test environments currently
    for_each = var.environment_short == "d" || var.environment_short == "t" ? [1] : []
    content {
      name                       = "IBA-P2S"
      priority                   = 1040
      direction                  = "Inbound"
      access                     = "Allow"
      protocol                   = "Tcp"
      source_port_range          = "*"
      destination_port_ranges    = ["443", "1433", "5671-5672"]
      source_address_prefix      = "10.143.198.64/26"
      destination_address_prefix = "VirtualNetwork"
    }
  }
  security_rule {
    name                       = "IBA-KafkaEventHub"
    priority                   = 1050
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "9093"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "VirtualNetwork"
  }
  security_rule {
    name                       = "deny_inbound_traffic"
    priority                   = 4096
    direction                  = "Inbound"
    access                     = "Deny"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "*"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }

  # OUTBOUND
  # Add all rules before the deny all rule
  security_rule {
    name                       = "deny_outbound_traffic"
    priority                   = 4096
    direction                  = "Outbound"
    access                     = "Deny"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "*"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }
  tags = local.tags
}

resource "azurerm_subnet_network_security_group_association" "privateendpoints" {
  subnet_id                 = azurerm_subnet.privateendpoints.id
  network_security_group_id = azurerm_network_security_group.privateendpoints.id
}
