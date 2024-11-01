module "kv_internal" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=key-vault_7.0.1"

  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  enabled_for_template_deployment = true
  enabled_for_deployment          = true
  sku_name                        = "premium"
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                        = local.ip_restrictions_as_string
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null
}

resource "azurerm_role_assignment" "microsoft_azure_website_access" {
  scope = module.kv_internal.id
  # principal_id is an app-registration 'Microsoft Azure Websites'
  # Reference: https://portal.azure.com/#view/Microsoft_AAD_IAM/ManagedAppMenuBlade/~/Overview/objectId/e383250e-d5d6-45b3-89f1-5321b821b063/appId/abfa0a7c-a6b6-4736-8310-5855508787cd
  principal_id         = "e383250e-d5d6-45b3-89f1-5321b821b063"
  role_definition_name = "Key Vault Secrets User"
}
