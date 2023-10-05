module "snet_apim" {
  count = 0
}

module "apim_shared" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management?ref=v13"

  subnet_id = data.azurerm_subnet.snet_apim.id
}
