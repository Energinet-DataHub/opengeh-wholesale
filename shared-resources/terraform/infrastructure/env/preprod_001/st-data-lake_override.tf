module "st_data_lake" {
  source                     = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v13"
  private_endpoint_subnet_id = data.azurerm_subnet.snet_private_endpoints.id
}
