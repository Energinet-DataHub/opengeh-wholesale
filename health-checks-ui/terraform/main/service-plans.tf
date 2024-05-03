module "webapp_service_plan" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-plan?ref=14.7.1"

  type                 = "webapp"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  sku_name             = "P0v3"
}
