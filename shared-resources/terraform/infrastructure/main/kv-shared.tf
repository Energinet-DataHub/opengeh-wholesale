module "kv_shared" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=key-vault_8.0.0"

  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  enabled_for_template_deployment = true
  sku_name                        = "premium"
  private_endpoint_subnet_id      = azurerm_subnet.privateendpoints.id
  ip_rules                        = local.ip_restrictions_as_string
  allowed_subnet_ids = [
    azurerm_subnet.vnetintegrations.id,
  ]
  audit_storage_account = var.enable_audit_logs ? {
    id = module.st_audit_logs.id
  } : null
}

# Used for authenticating to GitHub Container Registry
module "kvs_pat_token" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "github-pat-token"
  value        = var.git_pat
  key_vault_id = module.kv_shared.id
}
