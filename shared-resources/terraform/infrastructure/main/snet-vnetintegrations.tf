resource "azurerm_subnet" "vnetintegrations" {
  name                 = "snet-vnetintegrations"
  resource_group_name  = var.virtual_network_resource_group_name
  virtual_network_name = var.virtual_network_name
  address_prefixes     = var.vnetintegrations_address_prefixes

  delegation {
    name = "delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }

  service_endpoints = ["Microsoft.Storage", "Microsoft.KeyVault", "Microsoft.EventHub"]
}

resource "azurerm_route_table" "vnetintegrations" {
  name                = "rt-vnetintegrations"
  location            = data.azurerm_virtual_network.this.location
  resource_group_name = var.virtual_network_resource_group_name

  route {
    name           = "AzureActiveDirectory"
    address_prefix = "AzureActiveDirectory"
    next_hop_type  = "Internet"
  }
  route {
    name           = "udr-AzureCloud"
    address_prefix = "AzureCloud"
    next_hop_type  = "Internet"
  }
  route {
    name                   = "udr-azurefirewall"
    address_prefix         = "0.0.0.0/0"
    next_hop_type          = "VirtualAppliance"
    next_hop_in_ip_address = var.udr_firewall_next_hop_ip_address
  }
  route {
    name           = "udr-AzureMonitor"
    address_prefix = "AzureMonitor"
    next_hop_type  = "Internet"
  }

  tags = local.tags
}

resource "azurerm_subnet_route_table_association" "vnetintegrations" {
  subnet_id      = azurerm_subnet.vnetintegrations.id
  route_table_id = azurerm_route_table.vnetintegrations.id
}

resource "azurerm_network_security_group" "vnetintegrations" {
  name                = "nsg-vnetintegrations"
  location            = data.azurerm_virtual_network.this.location
  resource_group_name = var.virtual_network_resource_group_name

  # INBOUND
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
  # Firewall acts as DNS too
  security_rule {
    name                       = "OBA-DNS"
    priority                   = 900
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "53"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = var.udr_firewall_next_hop_ip_address
  }
  security_rule {
    name                       = "OBA-AllowHttpsToPeSubnet"
    priority                   = 1020
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "VirtualNetwork"
  }
  security_rule {
    name                       = "OBA-AllowAnyMS_SQLOutbound"
    priority                   = 1030
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "1443"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "VirtualNetwork"
  }
  security_rule {
    name                       = "OBA-AllowAMQPOutbound"
    priority                   = 1040
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "5671-5672"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "VirtualNetwork"
  }
  security_rule {
    name                       = "OBA-AAD-B2C-Allow"
    priority                   = 1050
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "AzureActiveDirectory"
  }
  security_rule {
    name                       = "OBA-AzureMonitor"
    priority                   = 1060
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "AzureMonitor"
  }
  security_rule {
    name                       = "OBA-AzureMonitorStorage"
    priority                   = 1070
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "Storage.WestEurope"
  }
  security_rule {
    name                       = "OBA-AzureEventHub"
    priority                   = 1080
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "5671-5672"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "EventHub"
  }
  security_rule {
    name                       = "OBA-Internet"
    priority                   = 1090
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_ranges    = ["80", "443"]
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "Internet"
  }
  security_rule {
    name                       = "OBA-VirkElasticSearch"
    priority                   = 2000
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "8443"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "Internet"
  }
  security_rule {
    name                       = "OBA-SMBFileShare"
    priority                   = 2010
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "445"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "Internet"
  }
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

resource "azurerm_subnet_network_security_group_association" "vnetintegrations" {
  subnet_id                 = azurerm_subnet.vnetintegrations.id
  network_security_group_id = azurerm_network_security_group.vnetintegrations.id
}
