module "st_testdata" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=storage-account_4.0.1"

  name                       = "testdata"
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
      name = "testdata"
    },
  ]
  role_assignments = [
    {
      principal_id         = data.azurerm_client_config.current.object_id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      principal_id         = data.azuread_group.developer_security_group_name.object_id
      role_definition_name = "Storage Blob Data Reader"
    }
  ]
}
