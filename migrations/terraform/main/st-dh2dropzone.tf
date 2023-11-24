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
  ip_rules                        = var.datahub2_ip_whitelist != "" ? format("%s,%s", var.hosted_deployagent_public_ip_range, var.datahub2_ip_whitelist) : var.hosted_deployagent_public_ip_range
}

#---- Role assignments

resource "azurerm_role_assignment" "ra_dh2dropzone_contributor" {
  scope                = module.st_dh2dropzone.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.id
}

resource "azurerm_role_assignment" "ra_dh2_migration_dh2dropzone_contributor" {
  scope                = module.st_dh2dropzone.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_cgi_dh2_data_migration.id
}

resource "azurerm_role_assignment" "ra_ehdropzone_sender" {
  scope                = azurerm_eventhub_namespace.eventhub_namespace_dropzone.id
  role_definition_name = "Azure Event Hubs Data Sender"
  principal_id         = azurerm_eventgrid_system_topic.system_topic_dropzone_zipped.identity[0].principal_id
}

#---- Containers

resource "azurerm_storage_container" "dh2_dropzone_zipped" {
  name                  = "dh2-dropzone-zipped"
  storage_account_name  = module.st_dh2dropzone.name
  container_access_type = "private"
}

#---- System Topic for all storage account events

resource "azurerm_eventgrid_system_topic" "system_topic_dropzone_zipped" {
  name                   = "est-dropzonezipped-${local.resources_suffix}"
  resource_group_name    = azurerm_resource_group.this.name
  location               = azurerm_resource_group.this.location
  source_arm_resource_id = module.st_dh2dropzone.id
  topic_type             = "Microsoft.Storage.StorageAccounts"
  identity {
    type = "SystemAssigned"
  }
}

#---- System topic event subscription for blob created events
resource "azurerm_eventgrid_system_topic_event_subscription" "eventgrid_dropzone_zipped" {
  name                 = "ests-dropzonezipped-${local.resources_suffix}"
  system_topic         = azurerm_eventgrid_system_topic.system_topic_dropzone_zipped.name
  resource_group_name  = azurerm_resource_group.this.name
  included_event_types = ["Microsoft.Storage.BlobCreated"]
  eventhub_endpoint_id = azurerm_eventhub.eventhub_dropzone_zipped.id
  subject_filter {
    subject_begins_with = "/blobServices/default/containers/dh2-dropzone-zipped"
  }
  delivery_identity {
    type = "SystemAssigned"
  }
}
