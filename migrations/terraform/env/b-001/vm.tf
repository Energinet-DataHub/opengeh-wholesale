locals {
  agents_count = 1
  name_suffix = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
}

module "snet_virtualmachine" {
  source               = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/subnet?ref=v12"
  name                 = "copyagents"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = var.virtual_network_resource_group_name
  virtual_network_name = data.azurerm_virtual_network.this.name
  address_prefixes = [
    var.agents_address_space
  ]
  service_endpoints = [
    "Microsoft.KeyVault"
  ]
}

# Create public IP
resource "azurerm_public_ip" "agent" {
  count               = local.agents_count
  name                = "pip-agent${count.index}-${local.name_suffix}"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  allocation_method   = "Static"
  ip_tags             = {}

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}

# Create network interface
resource "azurerm_network_interface" "agent" {
  count               = local.agents_count
  name                = "nic-agent${count.index}-${local.name_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

  ip_configuration {
    name                          = "primary"
    subnet_id                     = module.snet_virtualmachine.id
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id          = azurerm_public_ip.agent[count.index].id
  }

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}

# Create network security group and rules
resource "azurerm_network_security_group" "agent" {
  name                = "nsg-agent-${local.name_suffix}"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}

resource "azurerm_network_security_rule" "agent" {
  name                        = "SSH"
  priority                    = 100
  direction                   = "Inbound"
  access                      = "Allow"
  protocol                    = "Tcp"
  source_port_range           = "*"
  source_address_prefix       = "*"
  destination_port_range      = "22"
  destination_address_prefix  = "*"
  resource_group_name         = azurerm_resource_group.this.name
  network_security_group_name = azurerm_network_security_group.agent.name
}

resource "null_resource" "remove_agent_security_rule" {
  triggers = {
    always_run = "${timestamp()}"
  }

  provisioner "local-exec" {
    command     = "az resource delete --ids ${resource.azurerm_network_security_rule.agent.id}"
    interpreter = ["pwsh", "-Command"]
  }
  depends_on = [
    azurerm_network_security_rule.agent,
    azurerm_linux_virtual_machine.agent,
  ]
}

# Connect the security group to the network interface
resource "azurerm_network_interface_security_group_association" "agent" {
  count                     = local.agents_count
  network_interface_id      = azurerm_network_interface.agent[count.index].id
  network_security_group_id = azurerm_network_security_group.agent.id
}

# Generate random text for a unique storage account name
resource "random_id" "storageid" {
  keepers = {
    # Generate a new ID only when a new resource group is defined
    resource_group = azurerm_resource_group.this.name
  }

  byte_length = 8
}

# Create storage account for boot diagnostics
resource "azurerm_storage_account" "agent" {
  count                    = local.agents_count
  name                     = "stdiag${random_id.storageid.hex}${count.index}"
  resource_group_name      = azurerm_resource_group.this.name
  location                 = azurerm_resource_group.this.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}

# Create VM password
resource "random_password" "vmpassword" {
  length           = 20
  special          = true
  override_special = "_%@"
}

# Create virtual machine
# Notice: OS disk is automatically deleted whenever the VM is deleted
resource "azurerm_linux_virtual_machine" "agent" {
  count                           = local.agents_count
  name                            = "vm-agent${count.index}-${local.name_suffix}"
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  size                            = "Standard_DS5_v2"
  admin_username                  = var.vm_user_name
  admin_password                  = random_password.vmpassword.result
  disable_password_authentication = false
  network_interface_ids = [
    azurerm_network_interface.agent[count.index].id
  ]

  # Changes to the script file means the VM will be recreated
  custom_data = filebase64sha256("${path.module}/scripts/setup-agent.sh")

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }

  # Enable managed identity
  identity {
    identity_ids = []
    type         = "SystemAssigned"
  }

  os_disk {
    name                 = "osdisk-agent${count.index}-${local.name_suffix}"
    caching              = "ReadWrite"
    storage_account_type = "StandardSSD_LRS"
  }

  source_image_reference {
    publisher = "Canonical"
    offer     = "UbuntuServer"
    sku       = "18.04-LTS"
    version   = "latest"
  }

  boot_diagnostics {
    storage_account_uri = azurerm_storage_account.agent[count.index].primary_blob_endpoint
  }

  connection {
    type     = "ssh"
    user     = var.vm_user_name
    password = random_password.vmpassword.result
    host     = azurerm_public_ip.agent[count.index].ip_address
  }

  provisioner "file" {
    source      = "${path.module}/scripts/setup-agent.sh"
    destination = "setup-agent.sh"
  }

  # We install some software but not the GitHub runner. Volt will run scripts using their own user so we don't need the runner.
  provisioner "remote-exec" {
    inline = [
      "chmod +x ./setup-agent.sh",
      "./setup-agent.sh",
    ]
  }
}
