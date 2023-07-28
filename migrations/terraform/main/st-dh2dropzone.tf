module "st_dh2dropzone" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v12"

  name                            = "dh2dropzone"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  account_replication_type        = "LRS"
  account_tier                    = "Standard"
  access_tier                     = "Hot"
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  private_dns_resource_group_name = var.shared_resources_resource_group_name
  ip_rules                        = var.hosted_deployagent_public_ip_range
}

#---- Role assignments

resource "azurerm_role_assignment" "ra_dh2dropzone_contributor" {
  scope                = module.st_dh2dropzone.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.id
}

#---- Containers

resource "azurerm_storage_container" "dh2_dropzone_zipped" {
  name                  = "dh2-dropzone-zipped"
  storage_account_name  = module.st_dh2dropzone.name
  container_access_type = "private"
}

#---- Event Grid to trigger Event Hub (Must be in this scope, to react on blob creation in the container)

resource "azurerm_eventgrid_event_subscription" "eventhub_dropzone_zipped_trigger" {
  name                 = "eh-dropzone-zipped-trigger"
  scope                = module.st_dh2dropzone.id
  included_event_types = ["Microsoft.Storage.BlobCreated"]
  eventhub_endpoint_id = module.eventhub_dropzone_zipped.id
  subject_filter {
    subject_begins_with = "/blobServices/default/containers/dh2-dropzone-zipped"
  }
}
