module "kv_shared" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=v10"

  name                            = "main"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  enabled_for_template_deployment = true
  sku_name                        = "premium"
  log_analytics_workspace_id      = module.log_workspace_shared.id
  private_endpoint_subnet_id      = module.snet_private_endpoints.id
  allowed_subnet_ids              = [
    module.snet_vnet_integrations.id,
    data.azurerm_subnet.deployment_agents_subnet.id
  ]
  ip_rules = var.hosted_deployagent_public_ip_range
}

module "kvs_pir_hosted_deployment_agents" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "pir-hosted-deployment-agents"
  value         = var.hosted_deployagent_public_ip_range
  key_vault_id  = module.kv_shared.id
}