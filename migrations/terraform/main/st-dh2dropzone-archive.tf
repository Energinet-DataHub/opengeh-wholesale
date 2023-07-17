module "st_dh2dropzone_archive" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v12"

  name                            = "dh2dropzonearch"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  account_replication_type        = "LRS"
  account_tier                    = "Standard"
  access_tier                     = "Cool"
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  private_dns_resource_group_name = var.shared_resources_resource_group_name
  ip_rules                        = data.azurerm_key_vault_secret.pir_hosted_deployment_agents.value
}

#---- Role assignments

resource "azurerm_role_assignment" "ra_dh2dropzonearch_contributor" {
  scope                = module.st_dh2dropzone_archive.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.id
}

#---- Containers

resource "azurerm_storage_container" "dropzonearchive" {
  name                  = "dropzonearchive"
  storage_account_name  = module.st_dh2dropzone_archive.name
  container_access_type = "private"
}
