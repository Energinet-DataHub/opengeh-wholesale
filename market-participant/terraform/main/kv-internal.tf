module "kv_internal" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=v10"

  name                            = "int"
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
    data.azurerm_key_vault_secret.snet_vnet_integrations_id.value,
  ]
  access_policies                 = [
    {
      object_id               = module.func_entrypoint_marketparticipant.identity.0
      secret_permissions      = ["List", "Get"]
      key_permissions         = ["List", "Get", "Sign"]
    }
  ]
}

module "kvs_sendgrid_api_key" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "sendgrid-api-key"
  value         = var.sendgrid_api_key
  key_vault_id  = module.kv_internal.id
}

module "kvs_sendgrid_from_email" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "sendgrid-from-email"
  value         = var.sendgrid_from_email
  key_vault_id  = module.kv_internal.id
}

module "kvs_sendgrid_bcc_email" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "sendgrid-bcc-email"
  value         = var.sendgrid_bcc_email
  key_vault_id  = module.kv_internal.id
}
