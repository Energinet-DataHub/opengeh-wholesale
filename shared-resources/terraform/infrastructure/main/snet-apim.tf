resource "azurerm_subnet" "apim" {
  name                 = "snet-apim"
  resource_group_name  = var.virtual_network_resource_group_name
  virtual_network_name = var.virtual_network_name
  address_prefixes     = var.apim_address_prefixes

  service_endpoints = ["Microsoft.KeyVault", "Microsoft.Storage"]
}

resource "azurerm_route_table" "apim" {
  name                = "rt-apim"
  location            = data.azurerm_virtual_network.this.location
  resource_group_name = var.virtual_network_resource_group_name

  route {
    name           = "ApiManagement"
    address_prefix = "ApiManagement.WestEurope"
    next_hop_type  = "Internet"
  }
  route {
    name           = "AzureActiveDirectory"
    address_prefix = "AzureActiveDirectory"
    next_hop_type  = "Internet"
  }
  route {
    name           = "AzureMonitor"
    address_prefix = "AzureMonitor"
    next_hop_type  = "Internet"
  }
  route {
    name           = "udr-APIM-PublicIP"
    address_prefix = "AzureCloud.WestEurope"
    next_hop_type  = "Internet"
  }
  route {
    name           = "udr-AzureStorage"
    address_prefix = "Storage"
    next_hop_type  = "Internet"
  }
  route {
    name           = "udr-AzureTrafficManager"
    address_prefix = "AzureTrafficManager"
    next_hop_type  = "Internet"
  }
  route {
    name                   = "udr-Local-AzFirewall1"
    address_prefix         = "10.0.0.0/8"
    next_hop_type          = "VirtualAppliance"
    next_hop_in_ip_address = var.apim_next_hop_ip_address
  }
  route {
    name                   = "udr-Local-AzFirewall2"
    address_prefix         = "172.16.0.0/12"
    next_hop_type          = "VirtualAppliance"
    next_hop_in_ip_address = var.apim_next_hop_ip_address
  }
  route {
    name                   = "udr-Local-AzFirewall3"
    address_prefix         = "192.168.0.0/16"
    next_hop_type          = "VirtualAppliance"
    next_hop_in_ip_address = var.apim_next_hop_ip_address
  }

  tags = local.tags
}

resource "azurerm_subnet_route_table_association" "apim" {
  subnet_id      = azurerm_subnet.apim.id
  route_table_id = azurerm_route_table.apim.id
}

resource "azurerm_network_security_group" "apim" {
  name                = "nsg-apim"
  location            = data.azurerm_virtual_network.this.location
  resource_group_name = var.virtual_network_resource_group_name

  # INBOUND
  security_rule {
    name                       = "IBA-Client-HTTPS-VNET"
    priority                   = 1000
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_ranges    = ["80", "443"]
    source_address_prefix      = "Internet"
    destination_address_prefix = "VirtualNetwork"
  }
  security_rule {
    name                       = "IBA-Management_endpoint_for_Azure_portal_and_PowerShell"
    priority                   = 1010
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "3443"
    source_address_prefix      = "ApiManagement"
    destination_address_prefix = "VirtualNetwork"
  }
  security_rule {
    name                       = "IBA-Dependency_on_Azure_Load_Balancer"
    priority                   = 1020
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "*"
    source_address_prefix      = "AzureLoadBalancer"
    destination_address_prefix = "VirtualNetwork"
  }
  security_rule {
    name                       = "IBA-Dependency_on_Azure_Traffic_Manager"
    priority                   = 1030
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "*"
    source_address_prefix      = "AzureTrafficManager"
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
    name                       = "OBA-Dependency_on_Azure_SQL"
    priority                   = 1000
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "1433"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "Sql"
  }
  security_rule {
    name                       = "OBA-Publish_Monitoring_Logs"
    priority                   = 1010
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "AzureCloud"
  }
  security_rule {
    name                       = "OBA-Access_KeyVault"
    priority                   = 1020
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "AzureKeyVault"
  }
  security_rule {
    name                       = "OBA-Access_AD"
    priority                   = 1030
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "AzureActiveDirectory"
  }
  security_rule {
    name                       = "OBA-Access_AzureConnections"
    priority                   = 1040
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "AzureConnectors"
  }
  security_rule {
    name                       = "OBA-Dependency_for_Log_to_event_Hub_policy"
    priority                   = 1050
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_ranges    = ["443", "5672"]
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "EventHub"
  }
  security_rule {
    name                       = "OBA-Dependency_for_AzureMonitor"
    priority                   = 1060
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_ranges    = ["443", "1886"]
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "AzureMonitor"
  }
  security_rule {
    name                       = "OBA-VNET-HTTPS"
    priority                   = 1070
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "VirtualNetwork"
  }
  security_rule {
    name                       = "OBA-APIM-Storage"
    priority                   = 1080
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "*"
    destination_address_prefix = "Storage"
  }
  security_rule {
    name                       = "OBA-APIM-TrafficManager"
    priority                   = 1090
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "443"
    source_address_prefix      = "*"
    destination_address_prefix = "AzureTrafficManager"
  }
  security_rule {
    name                       = "OBA-TLS-Certificate"
    priority                   = 1100
    direction                  = "Outbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_ranges    = ["80", "443"]
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

resource "azurerm_subnet_network_security_group_association" "apim" {
  subnet_id                 = azurerm_subnet.apim.id
  network_security_group_id = azurerm_network_security_group.apim.id
}
