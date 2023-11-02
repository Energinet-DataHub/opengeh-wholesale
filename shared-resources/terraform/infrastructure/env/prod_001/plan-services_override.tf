module "plan_services" {
  source   = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service-plan?ref=v13"
  sku_name = "P3v3"
}
