module "st_process_manager" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=storage-account_8.0.0"

  name                 = "pmtaskhub"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  # https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-storage-providers
  account_replication_type              = "GRS"
  private_endpoint_subnet_id            = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  ip_rules                              = local.ip_restrictions_as_string
  use_table                             = true
  use_queue                             = true
  use_blob                              = true
  shared_access_key_enabled             = true
  lifecycle_retention_delete_after_days = 90
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null
}
