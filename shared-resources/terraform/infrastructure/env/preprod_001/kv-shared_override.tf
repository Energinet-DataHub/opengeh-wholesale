module "kv_shared" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=v13"

  private_endpoint_subnet_id = data.azurerm_subnet.snet_private_endpoints.id
  allowed_subnet_ids = [
    data.azurerm_subnet.snet_vnet_integration.id,
  ]
}
