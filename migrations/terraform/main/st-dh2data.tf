module "st_dh2data" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v13"

  name                       = "dh2data"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "LRS"
  account_tier               = "Standard"
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                   = local.ip_restrictions_as_string
  prevent_deletion           = true
}

#---- System Topic for all storage account events

resource "azurerm_eventgrid_system_topic" "st_dh2data" {
  name                   = "egst-${module.st_dh2data.name}-${local.resources_suffix}"
  resource_group_name    = azurerm_resource_group.this.name
  location               = azurerm_resource_group.this.location
  source_arm_resource_id = module.st_dh2data.id
  topic_type             = "Microsoft.Storage.StorageAccounts"
  identity {
    type = "SystemAssigned"
  }
}

#---- Role assignments

resource "azurerm_role_assignment" "ra_dh2data_contributor" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.id
}

resource "azurerm_role_assignment" "ra_dh2data_queue_data_contributor" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Queue Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.id
}

resource "azurerm_role_assignment" "ra_dh2data_queue_data_reader" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Queue Data Reader"
  principal_id         = azuread_service_principal.spn_databricks.id
}

resource "azurerm_role_assignment" "ra_dh2data_queue_data_message_processor" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Queue Data Message Processor"
  principal_id         = azuread_service_principal.spn_databricks.id
}

resource "azurerm_role_assignment" "ra_dh2data_blob_data_owner" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Blob Data Owner"
  principal_id         = azuread_service_principal.spn_databricks.id
}

resource "azurerm_role_assignment" "st_dh2data_queue_data_sender" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Queue Data Message Sender"
  principal_id         = azurerm_eventgrid_system_topic.st_dh2data.identity[0].principal_id
}

#---- Containers

resource "azurerm_storage_container" "dh2_metering_point_history" {
  name                  = "dh2-metering-point-history"
  storage_account_name  = module.st_dh2data.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "dh2_timeseries" {
  name                  = "dh2-timeseries"
  storage_account_name  = module.st_dh2data.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "dh2_timeseries_synchronization" {
  name                  = "dh2-timeseries-synchronization"
  storage_account_name  = module.st_dh2data.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "dh2_charges" {
  name                  = "dh2-charges"
  storage_account_name  = module.st_dh2data.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "dh2_charge_links" {
  name                  = "dh2-charge-links"
  storage_account_name  = module.st_dh2data.name
  container_access_type = "private"
}

#---- Queues

resource "azurerm_storage_queue" "charges" {
  name                 = azurerm_storage_container.dh2_charges.name
  storage_account_name = module.st_dh2data.name
}

resource "azurerm_storage_queue" "charge_links" {
  name                 = azurerm_storage_container.dh2_charge_links.name
  storage_account_name = module.st_dh2data.name
}

resource "azurerm_storage_queue" "metering_point_history" {
  name                 = azurerm_storage_container.dh2_metering_point_history.name
  storage_account_name = module.st_dh2data.name
}

resource "azurerm_storage_queue" "timeseries" {
  name                 = azurerm_storage_container.dh2_timeseries.name
  storage_account_name = module.st_dh2data.name
}

resource "azurerm_storage_queue" "timeseries_synchronization" {
  name                 = azurerm_storage_container.dh2_timeseries_synchronization.name
  storage_account_name = module.st_dh2data.name
}

#---- System topic event subscriptions

resource "azurerm_eventgrid_system_topic_event_subscription" "dh2data_charges" {
  name                 = "egsts-${azurerm_storage_queue.charges.name}-${local.resources_suffix}"
  system_topic         = azurerm_eventgrid_system_topic.st_dh2data.name
  resource_group_name  = azurerm_resource_group.this.name
  included_event_types = ["Microsoft.Storage.BlobCreated"]

  subject_filter {
    subject_begins_with = "/blobServices/default/containers/${azurerm_storage_container.dh2_charges.name}/"
  }

  storage_queue_endpoint {
    storage_account_id = module.st_dh2data.id
    queue_name         = azurerm_storage_queue.charges.name
  }
  delivery_identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_eventgrid_system_topic_event_subscription" "dh2data_charge_links" {
  name                 = "egsts-${azurerm_storage_queue.charge_links.name}-${local.resources_suffix}"
  system_topic         = azurerm_eventgrid_system_topic.st_dh2data.name
  resource_group_name  = azurerm_resource_group.this.name
  included_event_types = ["Microsoft.Storage.BlobCreated"]

  subject_filter {
    subject_begins_with = "/blobServices/default/containers/${azurerm_storage_container.dh2_charge_links.name}/"
  }

  storage_queue_endpoint {
    storage_account_id = module.st_dh2data.id
    queue_name         = azurerm_storage_queue.charge_links.name
  }
  delivery_identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_eventgrid_system_topic_event_subscription" "dh2_metering_point_history" {
  name                 = "egsts-${azurerm_storage_queue.metering_point_history.name}-${local.resources_suffix}"
  system_topic         = azurerm_eventgrid_system_topic.st_dh2data.name
  resource_group_name  = azurerm_resource_group.this.name
  included_event_types = ["Microsoft.Storage.BlobCreated"]

  subject_filter {
    subject_begins_with = "/blobServices/default/containers/${azurerm_storage_container.dh2_metering_point_history.name}/"
  }

  storage_queue_endpoint {
    storage_account_id = module.st_dh2data.id
    queue_name         = azurerm_storage_queue.metering_point_history.name
  }
  delivery_identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_eventgrid_system_topic_event_subscription" "dh2_timeseries" {
  name                 = "egsts-${azurerm_storage_queue.timeseries.name}-${local.resources_suffix}"
  system_topic         = azurerm_eventgrid_system_topic.st_dh2data.name
  resource_group_name  = azurerm_resource_group.this.name
  included_event_types = ["Microsoft.Storage.BlobCreated"]

  subject_filter {
    subject_begins_with = "/blobServices/default/containers/${azurerm_storage_container.dh2_timeseries.name}/"
  }

  storage_queue_endpoint {
    storage_account_id = module.st_dh2data.id
    queue_name         = azurerm_storage_queue.timeseries.name
  }
  delivery_identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_eventgrid_system_topic_event_subscription" "dh2_timeseries_synchronization" {
  name                 = "egsts-${azurerm_storage_queue.timeseries.name}-${local.resources_suffix}"
  system_topic         = azurerm_eventgrid_system_topic.st_dh2data.name
  resource_group_name  = azurerm_resource_group.this.name
  included_event_types = ["Microsoft.Storage.BlobCreated"]

  subject_filter {
    subject_begins_with = "/blobServices/default/containers/${azurerm_storage_container.dh2_timeseries_synchronization.name}/"
  }

  storage_queue_endpoint {
    storage_account_id = module.st_dh2data.id
    queue_name         = azurerm_storage_queue.timeseries_synchronization.name
  }
  delivery_identity {
    type = "SystemAssigned"
  }
}
