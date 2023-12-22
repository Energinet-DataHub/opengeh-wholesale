module "kv_dh2_certificates" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=v13"

  name                            = "cert"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  enabled_for_template_deployment = true
  sku_name                        = "premium"
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                        = var.hosted_deployagent_public_ip_range
}
