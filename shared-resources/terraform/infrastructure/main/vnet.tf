data "azurerm_virtual_network" "this" {
  name                = var.virtual_network_name
  resource_group_name = var.virtual_network_resource_group_name
}

data "azurerm_subnet" "deployment_agents_subnet" {
  name                  = var.deployment_agents_subnet_name
  virtual_network_name  = var.virtual_network_name
  resource_group_name   = var.virtual_network_resource_group_name
}

module "kvs_vnet_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "vnet-name"
  value         = data.azurerm_virtual_network.this.name
  key_vault_id  = module.kv_shared.id
}

module "kvs_vnet_id" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "vnet-id"
  value         = data.azurerm_virtual_network.this.id
  key_vault_id  = module.kv_shared.id
}

module "kvs_vnet_resource_group_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "vnet-resource-group-name"
  value         = data.azurerm_virtual_network.this.resource_group_name
  key_vault_id  = module.kv_shared.id
}

module "snet_private_endpoints" {
  source                                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/subnet?ref=v10"
  name                                            = "private-endpoints"
  project_name                                    = var.domain_name_short
  environment_short                               = var.environment_short
  environment_instance                            = var.environment_instance
  resource_group_name                             = var.virtual_network_resource_group_name
  virtual_network_name                            = data.azurerm_virtual_network.this.name
  address_prefixes                                = [
    var.private_endpoint_address_space
  ]
  enforce_private_link_endpoint_network_policies  = true
  enforce_private_link_service_network_policies   = true
}

module "snet_vnet_integration" {
  source                                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/subnet?ref=v10"
  name                                            = "vnet-integration"
  project_name                                    = var.domain_name_short
  environment_short                               = var.environment_short
  environment_instance                            = var.environment_instance
  resource_group_name                             = var.virtual_network_resource_group_name
  virtual_network_name                            = data.azurerm_virtual_network.this.name
  address_prefixes                                = [
    var.vnet_integration_address_space
  ]
  enforce_private_link_service_network_policies   = true

  # Delegate the subnet to "Microsoft.Web/serverFarms"
  delegations =  [{
    name                        = "delegation"
    service_delegation_name     = "Microsoft.Web/serverFarms"
    service_delegation_actions  = [
      "Microsoft.Network/virtualNetworks/subnets/action"
    ]
  }]
  
  service_endpoints                               = [
    "Microsoft.KeyVault",
    "Microsoft.EventHub"
  ]
}


module "kvs_snet_private_endpoints_id" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "snet-private-endpoints-id"
  value         = module.snet_private_endpoints.id
  key_vault_id  = module.kv_shared.id
}

module "kvs_snet_vnet_integration_id" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "snet-vnet-integration-id"
  value         = module.snet_vnet_integration.id
  key_vault_id  = module.kv_shared.id
}


// DEPRECATED, IS BEING REMOVED
module "snet_vnet_integrations" {
  source                                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/subnet?ref=v10"
  name                                            = "vnet-integrations"
  project_name                                    = var.domain_name_short
  environment_short                               = var.environment_short
  environment_instance                            = var.environment_instance
  resource_group_name                             = var.virtual_network_resource_group_name
  virtual_network_name                            = data.azurerm_virtual_network.this.name
  address_prefixes                                = [
    var.vnet_integrations_address_space
  ]
  enforce_private_link_service_network_policies   = true

  # Delegate the subnet to "Microsoft.Web/serverFarms"
  delegations =  [{
    name                        = "delegation"
    service_delegation_name     = "Microsoft.Web/serverFarms"
    service_delegation_actions  = [
      "Microsoft.Network/virtualNetworks/subnets/action"
    ]
  }]
  
  service_endpoints                               = [
    "Microsoft.KeyVault",
    "Microsoft.EventHub"
  ]
}

module "kvs_snet_vnet_integrations_id" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "snet-vnet-integrations-id"
  value         = module.snet_vnet_integrations.id
  key_vault_id  = module.kv_shared.id
}
