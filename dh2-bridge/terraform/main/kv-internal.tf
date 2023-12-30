module "kv_internal" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=v13"

  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  enabled_for_template_deployment = true
  sku_name                        = "premium"
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                        = local.ip_restrictions_as_string
}

module "kvs_sendgrid_api_key" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "sendgrid-api-key"
  value        = var.sendgrid_api_key
  key_vault_id = module.kv_internal.id
}

module "kvs_sendgrid_to_email" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "sendgrid-to-email"
  value        = var.sendgrid_to_email
  key_vault_id = module.kv_internal.id
}

module "kvs_sendgrid_from_email" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "sendgrid-from-email"
  value        = var.sendgrid_from_email
  key_vault_id = module.kv_internal.id
}
