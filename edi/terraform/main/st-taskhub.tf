module "taskhub_storage_account" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=storage-account_6.3.0"

  name                       = "taskhub"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "GRS"
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_private_endpoints_002_id.value
  ip_rules                   = local.ip_restrictions_as_string
  use_table                  = true
  use_queue                  = true
  use_blob                   = true
  shared_access_key_enabled  = true
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null
}
