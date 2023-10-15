module "kv_esett" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=v13-without-vnet"

  name                            = "int"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  enabled_for_template_deployment = true
  enabled_for_deployment          = true
  sku_name                        = "standard"
}
