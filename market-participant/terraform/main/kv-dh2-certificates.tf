module "kv_dh2_certificates" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=key-vault_8.0.0"

  name                            = "cert"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  enabled_for_template_deployment = true
  sku_name                        = "premium"
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  ip_rules                        = local.ip_restrictions_as_string
  take_backup                     = true
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null
}

resource "azurerm_role_assignment" "kv_dh2_certificates_access_policy_apim_secrets_user" {
  scope                = module.kv_dh2_certificates.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = data.azurerm_key_vault_secret.apim_principal_id.value
}
