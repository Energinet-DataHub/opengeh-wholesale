module "mssql_data" {
  source                     = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/mssql-server?ref=v13"
  private_endpoint_subnet_id = data.azurerm_subnet.snet_private_endpoints.id
}
