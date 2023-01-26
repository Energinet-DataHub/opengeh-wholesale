module "kv_internal" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=v10"

  name                            = "internal"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  enabled_for_template_deployment = true
  sku_name                        = "premium"
  log_analytics_workspace_id      = data.azurerm_key_vault_secret.log_shared_id.value
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  allowed_subnet_ids              = [
  ]
}
