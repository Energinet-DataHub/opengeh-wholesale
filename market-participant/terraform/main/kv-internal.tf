module "kv_internal" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=key-vault_7.0.1"

  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  enabled_for_template_deployment = true
  sku_name                        = "premium"
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                        = local.ip_restrictions_as_string
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null
}

module "kvs_sendgrid_api_key" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sendgrid-api-key"
  value        = var.sendgrid_api_key
  key_vault_id = module.kv_internal.id
}

module "kvs_sendgrid_from_email" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sendgrid-from-email"
  value        = var.sendgrid_from_email
  key_vault_id = module.kv_internal.id
}

module "kvs_sendgrid_bcc_email" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "sendgrid-bcc-email"
  value        = var.sendgrid_bcc_email
  key_vault_id = module.kv_internal.id
}

module "kvs_cvr_password" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "cvr-password"
  value        = var.cvr_password
  key_vault_id = module.kv_internal.id
}

module "kvs_mssql_market_participant_connection_string" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "mssql-market-participant-connection-string"
  value        = local.CONNECTION_STRING_DB_MIGRATIONS
  key_vault_id = module.kv_internal.id
}
