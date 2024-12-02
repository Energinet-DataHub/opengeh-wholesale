module "kv_internal" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault?ref=key-vault_7.1.0"

  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  sku_name                   = "premium"
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  ip_rules                   = local.ip_restrictions_as_string
  audit_storage_account      = null # Not needed, only secret is a DB connectionstring used for database migrations
}
