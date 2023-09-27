module "kvs_snet_private_endpoints_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  value = data.azurerm_subnet.snet_private_endpoints.id
}

module "kvs_snet_vnet_integration_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  value = data.azurerm_subnet.snet_vnet_integration.id
}

module "snet_private_endpoints" { # Delete when old subscriptions are deleted
  count = 0 # Count is set to zero to ensure we don't create any subnets, since CA delivers this
}

module "snet_vnet_integration" { # Delete when old subscriptions are deleted
  count = 0 # Count is set to zero to ensure we don't create any subnets, since CA delivers this
}
