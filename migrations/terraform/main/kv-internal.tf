module "kv_internal" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=v13"

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
  enable_rbac_authorization       = true
}

resource "azurerm_role_assignment" "microsoft_azure_website_access" {
  count                = var.developers_security_group_object_id == null ? 0 : 1
  scope                = module.kv_internal.id
  principal_id         = "e383250e-d5d6-45b3-89f1-5321b821b063"
  role_definition_name = "Key Vault Secrets User"
}

resource "azurerm_role_assignment" "developer_access" {
  count                = var.developers_security_group_object_id == null ? 0 : 1
  scope                = module.kv_internal.id
  principal_id         = var.developers_security_group_object_id
  role_definition_name = "Key Vault Secrets User"
}

resource "azurerm_role_assignment" "omada_developer_access" {
  count                = var.omada_developers_security_group_object_id == null ? 0 : 1
  scope                = module.kv_internal.id
  principal_id         = var.omada_developers_security_group_object_id
  role_definition_name = "Key Vault Secrets User"
}
