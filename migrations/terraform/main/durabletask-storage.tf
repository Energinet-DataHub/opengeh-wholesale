module "durabletask_storage" {
  source                      = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/durabletask-storage-account?ref=durabletask-storage-account_1.0.0"

  project_name                = var.domain_name_short
  environment_short           = var.environment_short
  environment_instance        = var.environment_instance
  resource_group_name         = azurerm_resource_group.this.name
  location                    = azurerm_resource_group.this.location
  private_endpoint_subnet_id  = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
}
