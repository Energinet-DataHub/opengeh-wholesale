resource "azurerm_data_factory" "this" {
  name                            = "adf-${local.resources_suffix}"
  location                        = azurerm_resource_group.this.location
  resource_group_name             = azurerm_resource_group.this.name
  managed_virtual_network_enabled = true
  public_network_enabled          = false
  identity {
    type = "SystemAssigned"
  }

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  tags = local.tags
}

module "kvs_azure_data_factory_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=14.19.1"

  name         = "adf-id"
  value        = azurerm_data_factory.this.id
  key_vault_id = module.kv_shared.id
}

module "kvs_azure_data_factory_principal_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=14.19.1"

  name         = "adf-principal-id"
  value        = azurerm_data_factory.this.identity[0].principal_id
  key_vault_id = module.kv_shared.id
}
