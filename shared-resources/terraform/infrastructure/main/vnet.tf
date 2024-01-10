data "azurerm_virtual_network" "this" {
  name                = var.virtual_network_name
  resource_group_name = var.virtual_network_resource_group_name
}

data "azurerm_subnet" "snet_private_endpoints" {
  name                 = "snet-privateendpoint-we-001"
  virtual_network_name = var.virtual_network_name
  resource_group_name  = var.virtual_network_resource_group_name
}

data "azurerm_subnet" "snet_vnet_integration" {
  name                 = "snet-vnetintegrations-shres"
  virtual_network_name = var.virtual_network_name
  resource_group_name  = var.virtual_network_resource_group_name
}

data "azurerm_subnet" "snet_apim" {
  name                 = "snet-apim-shres"
  virtual_network_name = var.virtual_network_name
  resource_group_name  = var.virtual_network_resource_group_name
}

module "kvs_vnet_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "vnet-name"
  value        = data.azurerm_virtual_network.this.name
  key_vault_id = module.kv_shared.id
}

module "kvs_vnet_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "vnet-id"
  value        = data.azurerm_virtual_network.this.id
  key_vault_id = module.kv_shared.id
}

module "kvs_vnet_resource_group_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "vnet-resource-group-name"
  value        = data.azurerm_virtual_network.this.resource_group_name
  key_vault_id = module.kv_shared.id
}

module "kvs_snet_private_endpoints_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "snet-private-endpoints-id"
  value        = data.azurerm_subnet.snet_private_endpoints.id
  key_vault_id = module.kv_shared.id
}

module "kvs_snet_vnet_integration_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "snet-vnet-integration-id"
  value        = data.azurerm_subnet.snet_vnet_integration.id
  key_vault_id = module.kv_shared.id
}
