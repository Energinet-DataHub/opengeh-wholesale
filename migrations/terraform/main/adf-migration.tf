#  The private endpoint has been approved using azapi_resource and azapi_update_resource

resource "azurerm_data_factory" "this" {
  name                            = "adf-${local.resources_suffix}"
  location                        = azurerm_resource_group.this.location
  resource_group_name             = azurerm_resource_group.this.name
  managed_virtual_network_enabled = true
  public_network_enabled          = false
  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_data_factory_pipeline" "this" {
  name            = "pl-move-processed-files-${local.resources_suffix}"
  data_factory_id = azurerm_data_factory.this.id
  variables = {
    "thirtyOneDaysFromNow" = "string"
  }
  activities_json = templatefile("adf-pipelines/pl-move-processed-files.json",
    {
      stdh2data_md                = azurerm_data_factory_dataset_json.stdh2data_md.name
      stdh2data_md_processed      = azurerm_data_factory_dataset_json.stdh2data_md_processed.name
      stdh2data_ts                = azurerm_data_factory_dataset_json.stdh2data_ts.name
      stdh2data_ts_processed      = azurerm_data_factory_dataset_json.stdh2data_ts_processed.name
      stdh2data_ts_sync           = azurerm_data_factory_dataset_json.stdh2data_ts_sync.name
      stdh2data_ts_sync_processed = azurerm_data_factory_dataset_json.stdh2data_ts_sync_processed.name
  })
}

# Create datasets for the Data Factory

resource "azurerm_data_factory_dataset_json" "stdh2data_md" {
  name                = "stdh2data_md"
  data_factory_id     = azurerm_data_factory.this.id
  linked_service_name = azurerm_data_factory_linked_service_data_lake_storage_gen2.linked_service.name
  azure_blob_storage_location {
    container = azurerm_storage_container.dh2_metering_point_history.name
    path      = ""
    filename  = ""
  }
  encoding = "UTF-8"
}

resource "azurerm_data_factory_dataset_json" "stdh2data_md_processed" {
  name                = "stdh2data_md_processed"
  data_factory_id     = azurerm_data_factory.this.id
  linked_service_name = azurerm_data_factory_linked_service_data_lake_storage_gen2.linked_service.name
  azure_blob_storage_location {
    container = azurerm_storage_container.dh2_metering_point_history_processed.name
    path      = ""
    filename  = ""
  }
  encoding = "UTF-8"
}

resource "azurerm_data_factory_dataset_json" "stdh2data_ts" {
  name                = "stdh2data_ts"
  data_factory_id     = azurerm_data_factory.this.id
  linked_service_name = azurerm_data_factory_linked_service_data_lake_storage_gen2.linked_service.name
  azure_blob_storage_location {
    container = azurerm_storage_container.dh2_timeseries.name
    path      = ""
    filename  = ""
  }
  encoding = "UTF-8"
}

resource "azurerm_data_factory_dataset_json" "stdh2data_ts_processed" {
  name                = "stdh2data_ts_processed"
  data_factory_id     = azurerm_data_factory.this.id
  linked_service_name = azurerm_data_factory_linked_service_data_lake_storage_gen2.linked_service.name
  azure_blob_storage_location {
    container = azurerm_storage_container.dh2_timeseries_processed.name
    path      = ""
    filename  = ""
  }
  encoding = "UTF-8"
}

resource "azurerm_data_factory_dataset_json" "stdh2data_ts_sync" {
  name                = "stdh2data_ts_sync"
  data_factory_id     = azurerm_data_factory.this.id
  linked_service_name = azurerm_data_factory_linked_service_data_lake_storage_gen2.linked_service.name
  azure_blob_storage_location {
    container = azurerm_storage_container.dh2_timeseries_synchronization.name
    path      = ""
    filename  = ""
  }
  encoding = "UTF-8"
}

resource "azurerm_data_factory_dataset_json" "stdh2data_ts_sync_processed" {
  name                = "stdh2data_ts_sync_processed"
  data_factory_id     = azurerm_data_factory.this.id
  linked_service_name = azurerm_data_factory_linked_service_data_lake_storage_gen2.linked_service.name
  azure_blob_storage_location {
    container = azurerm_storage_container.dh2_timeseries_synchronization_processed.name
    path      = ""
    filename  = ""
  }
  encoding = "UTF-8"
}


#
# Ensure that the Data Factory has access to the storage account using managed identity and over private endpoint
#

resource "azurerm_role_assignment" "ra_dh2data_adf_contributor" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_data_factory.this.identity[0].principal_id
}

resource "azurerm_role_assignment" "ra_dh2dropzone_adf_contributor" {
  scope                = module.st_dh2dropzone.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_data_factory.this.identity[0].principal_id
}

resource "azurerm_data_factory_managed_private_endpoint" "adfpe_blob" {
  name               = "adfpe-blob-${local.resources_suffix}"
  data_factory_id    = azurerm_data_factory.this.id
  target_resource_id = module.st_dh2data.id
  subresource_name   = "blob"
}

resource "azurerm_data_factory_linked_service_data_lake_storage_gen2" "linked_service" {
  name                 = "ls_adls_${lower(module.st_dh2data.name)}"
  data_factory_id      = azurerm_data_factory.this.id
  use_managed_identity = true
  url                  = module.st_dh2data.fully_qualified_domain_name
}

# Approve private endpoints created by Azure Data Factory

# Get information about all private endpoint connections on the storage account
data "azapi_resource" "private_endpoint_connections" {
  type                   = "Microsoft.Storage/storageAccounts@2022-09-01"
  resource_id            = module.st_dh2data.id
  response_export_values = ["properties.privateEndpointConnections."]

  depends_on = [
    azurerm_data_factory_managed_private_endpoint.adfpe_blob
  ]
}

locals {
  private_endpoint_connection_name = try(one([
    for connection in jsondecode(data.azapi_resource.private_endpoint_connections.output).properties.privateEndpointConnections
    : connection.name
    if
    endswith(connection.properties.privateLinkServiceConnectionState.description, azurerm_data_factory_managed_private_endpoint.adfpe_blob.name)
  ]), null)
}

## Do the actual approval for each of the connections
resource "azapi_update_resource" "approve_private_endpoint_connection" {
  type      = "Microsoft.Storage/storageAccounts/privateEndpointConnections@2022-09-01"
  name      = local.private_endpoint_connection_name
  parent_id = module.st_dh2data.id

  body = jsonencode({
    properties = {
      privateLinkServiceConnectionState = {
        description = "Approved via Terraform - ${azurerm_data_factory_managed_private_endpoint.adfpe_blob.name}"
        status      = "Approved"
      }
    }
  })

  lifecycle {
    ignore_changes = all # We don't want to touch this after creation
  }
}
