module "kv_shared" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=v13"

  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  enabled_for_template_deployment = true
  sku_name                        = "premium"
  private_endpoint_subnet_id      = data.azurerm_subnet.snet_private_endpoints.id
  allowed_subnet_ids = [
    data.azurerm_subnet.snet_vnet_integration.id,
  ]
  ip_rules = var.hosted_deployagent_public_ip_range
}
