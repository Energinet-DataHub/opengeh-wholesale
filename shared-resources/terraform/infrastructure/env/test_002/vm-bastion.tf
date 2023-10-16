data "azurerm_subnet" "snet_bastion_shres" {
  name                 = "snet-bastion-shres"
  virtual_network_name = var.virtual_network_name
  resource_group_name  = var.virtual_network_resource_group_name
}

resource "azurerm_network_interface" "nic_bastion" {
  name                = "nic-bastion-${local.resources_suffix}"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name

  ip_configuration {
    name                          = "internal"
    subnet_id                     = data.azurerm_subnet.snet_bastion_shres.id
    private_ip_address_allocation = "Dynamic"
  }
}

resource "random_string" "this" {
  length = 14
}

resource "azurerm_linux_virtual_machine" "vm_bastion" {
  name                            = "vm-bastion-${local.resources_suffix}"
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  size                            = "Standard_DC1ds_v3"
  admin_username                  = "theoutlaws"
  admin_password                  = random_string.this.result
  disable_password_authentication = false
  user_data                       = filebase64("./scripts/initialize_bastion.sh")
  network_interface_ids = [
    azurerm_network_interface.nic_bastion.id,
  ]

  os_disk {
    caching              = "ReadWrite"
    storage_account_type = "Premium_LRS"
  }

  source_image_reference {
    publisher = "canonical"
    offer     = "0001-com-ubuntu-server-lunar"
    sku       = "23_04-gen2"
    version   = "latest"
  }

  identity {
    type = "SystemAssigned"
  }
}

# Sleep is needed to allow for script execution and SP creation
resource "time_sleep" "wait_300_seconds" {
  create_duration = "5m"
  depends_on      = [azurerm_linux_virtual_machine.vm_bastion]
}

data "azuread_service_principal" "bastion_sp" {
  object_id = azurerm_linux_virtual_machine.vm_bastion.identity[0].principal_id
  depends_on = [ time_sleep.wait_300_seconds ]
}

# Replace tenant and app id and ensure it always authenticates against azure Entra - needs to be on its own as it uses vars
# The sed command is used to replace the tenant_id and app_id in the aad.conf file - see https://github.com/ubuntu/aad-auth
resource "azurerm_virtual_machine_extension" "bastion_init" {
  name                 = "vm-ext-bastion-init-${local.resources_suffix}"
  virtual_machine_id   = azurerm_linux_virtual_machine.vm_bastion.id
  publisher            = "Microsoft.Azure.Extensions"
  type                 = "CustomScript"
  type_handler_version = "2.0"

  settings = <<SETTINGS
 {
  "commandToExecute": "sudo sed -i '6 a tenant_id = ${var.arm_tenant_id}' /etc/aad.conf && sudo sed -i '6 a app_id = ${data.azuread_service_principal.bastion_sp.application_id}' /etc/aad.conf && sudo sed -i '6 a offline_credentials_expiration = -1' /etc/aad.conf"
 }
SETTINGS
depends_on = [ data.azuread_service_principal.bastion_sp ]
}

