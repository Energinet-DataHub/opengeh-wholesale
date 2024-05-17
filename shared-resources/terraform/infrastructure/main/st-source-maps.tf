module "st_source_maps" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=14.3.0"

  name                       = "sourcemaps"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "LRS"
  access_tier                = "Hot"
  private_endpoint_subnet_id = data.azurerm_subnet.snet_private_endpoints.id
  ip_rules                   = local.ip_restrictions_as_string

  containers = [
    {
      name = "sourcemaps"
    },
  ]
  role_assignments = [
    {
      principal_id         = var.developers_security_group_object_id
      role_definition_name = "Storage Blob Data Reader"
    },
    {
      principal_id         = data.azurerm_client_config.current.object_id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      principal_id         = var.omada_developers_security_group_object_id
      role_definition_name = "Storage Queue Data Reader"
    }
  ]
}

module "kvs_st_source_maps_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=14.0.3"

  name         = "st-sourcemaps-name"
  value        = module.st_source_maps.name
  key_vault_id = module.kv_shared.id
}
