module "sb_domain_relay" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-namespace?ref=v13"
  private_endpoint_subnet_id    = data.azurerm_subnet.snet_private_endpoints.id
}
