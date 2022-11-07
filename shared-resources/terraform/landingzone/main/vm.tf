# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

locals {
  deployagent_count = 3
}


module "snet_deployagent" {
  source                = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/subnet?ref=7.0.0"
  name                  = "deployagents"
  project_name          = var.domain_name_short
  environment_short     = var.environment_short
  environment_instance  = var.environment_instance
  resource_group_name   = var.virtual_network_resource_group_name
  virtual_network_name  = data.azurerm_virtual_network.this.name
  address_prefixes      = [
    var.deployment_agent_address_space
  ]
  service_endpoints                               = [
    "Microsoft.KeyVault"
  ]
}

# Create public IP
resource "azurerm_public_ip" "deployagent" {
  count               = local.deployagent_count
  name                = "pip-deployagent${count.index}-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  allocation_method   = "Static"
  ip_tags             = {}

  tags                = azurerm_resource_group.this.tags

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}

# Create network interface
resource "azurerm_network_interface" "deployagent" {
  count               = local.deployagent_count
  name                = "nic-deployagent${count.index}-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

  ip_configuration {
    name                          = "primary"
    subnet_id                     = module.snet_deployagent.id
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id          = azurerm_public_ip.deployagent[count.index].id
  }

  tags                = azurerm_resource_group.this.tags

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}

# Create network security group and rules
resource "azurerm_network_security_group" "deployagent" {
  name                = "nsg-deployagent-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name

  tags                = azurerm_resource_group.this.tags

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }
}

resource "azurerm_network_security_rule" "deployagent" {
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
  network_security_group_name = azurerm_network_security_group.deployagent.name
}

resource "null_resource" "remove_deployagent_security_rule" {
  triggers = {
    always_run = "${timestamp()}"
  }

  provisioner "local-exec" {
    command     = "az resource delete --ids ${resource.azurerm_network_security_rule.deployagent.id}"
    interpreter = ["pwsh", "-Command"]
  }
  depends_on = [
    azurerm_network_security_rule.deployagent,
    azurerm_linux_virtual_machine.deployagent,
  ]
}

# Connect the security group to the network interface
resource "azurerm_network_interface_security_group_association" "deployagent" {
  count                     = local.deployagent_count
  network_interface_id      = azurerm_network_interface.deployagent[count.index].id
  network_security_group_id = azurerm_network_security_group.deployagent.id
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
resource "azurerm_storage_account" "deployagent" {
  count                       = local.deployagent_count
  name                        = "stdiag${random_id.storageid.hex}${count.index}"
  resource_group_name         = azurerm_resource_group.this.name
  location                    = azurerm_resource_group.this.location
  account_tier                = "Standard"
  account_replication_type    = "LRS"
  min_tls_version             = "TLS1_2"

  tags                        = azurerm_resource_group.this.tags

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
  length            = 20
  special           = true
  override_special  = "_%@"
}

# Create virtual machine
# Notice: OS disk is automatically deleted whenever the VM is deleted
resource "azurerm_linux_virtual_machine" "deployagent" {
  count                           = local.deployagent_count
  name                            = "vm-deployagent${count.index}-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  size                            = "Standard_DS2_v2"
  admin_username                  = var.vm_user_name
  admin_password                  = random_password.vmpassword.result
  disable_password_authentication = false
  network_interface_ids           = [
    azurerm_network_interface.deployagent[count.index].id
  ]

  # Changes to the script file means the VM will be recreated
  custom_data                     = filebase64sha256("${path.module}/scripts/setup-deploy-agent.sh")

  tags                            = azurerm_resource_group.this.tags

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }

  # Enable managed identity
  identity {
    identity_ids  = []
    type          = "SystemAssigned"
  }

  os_disk {
    name                 = "osdisk-deployagent${count.index}-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
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
    storage_account_uri = azurerm_storage_account.deployagent[count.index].primary_blob_endpoint
  }

  connection {
    type        = "ssh"
    user        = var.vm_user_name
    password    = random_password.vmpassword.result
    host        = azurerm_public_ip.deployagent[count.index].ip_address
  }

  provisioner "file" {
    source      = "${path.module}/scripts/setup-deploy-agent.sh"
    destination = "setup-deploy-agent.sh"
  }

  provisioner "remote-exec" {
    inline = [
      "chmod +x ./setup-deploy-agent.sh",
      "./setup-deploy-agent.sh ${var.github_runner_token} dplagent${count.index}-${lower(var.environment_short)}-${lower(var.environment_instance)} dplagent-${lower(var.environment_short)}-${lower(var.environment_instance)}",
    ]
  }
}
