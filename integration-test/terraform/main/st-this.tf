resource "azurerm_storage_account" "this" {
  name                     = "st${lower(var.domain_name_short)}${lower(var.environment_short)}we${lower(var.environment_instance)}"
  resource_group_name      = azurerm_resource_group.this.name
  location                 = azurerm_resource_group.this.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"
  is_hns_enabled           = true

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

#---- Roles
resource "azurerm_role_assignment" "ra_migrations_domain_test_contributor" {
  scope                = azurerm_storage_account.this.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_ci.id
}

resource "azurerm_role_assignment" "ra_migrations_queue_data_contributor" {
  scope                = azurerm_storage_account.this.id
  role_definition_name = "Storage Queue Data Contributor"
  principal_id         = azuread_service_principal.spn_ci.id
}

resource "azurerm_role_assignment" "ra_migrations_queue_data_reader" {
  scope                = azurerm_storage_account.this.id
  role_definition_name = "Storage Queue Data Reader"
  principal_id         = azuread_service_principal.spn_ci.id
}

resource "azurerm_role_assignment" "ra_migrations_queue_data_message_processor" {
  scope                = azurerm_storage_account.this.id
  role_definition_name = "Storage Queue Data Message Processor"
  principal_id         = azuread_service_principal.spn_ci.id
}

resource "azurerm_role_assignment" "ra_migrations_blob_data_owner" {
  scope                = azurerm_storage_account.this.id
  role_definition_name = "Storage Blob Data Owner"
  principal_id         = azuread_service_principal.spn_ci.id
}

resource "azurerm_role_assignment" "ra_migrations_queue_data_sender" {
  scope                = azurerm_storage_account.this.id
  role_definition_name = "Storage Queue Data Message Sender"
  principal_id         = azurerm_eventgrid_system_topic.this.identity[0].principal_id
}

#---- Containers
resource "azurerm_storage_container" "domaintest" {
  name                  = "domaintest"
  storage_account_name  = azurerm_storage_account.this.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "domain_test_timeseries_testdata" {
  name                  = "time-series-testdata"
  storage_account_name  = azurerm_storage_account.this.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "domain_test_meteringpoints_testdata" {
  name                  = "metering-points-testdata"
  storage_account_name  = azurerm_storage_account.this.name
  container_access_type = "private"
}

#---- Queues
resource "azurerm_storage_queue" "metering_point_history" {
  name                 = azurerm_storage_container.domain_test_meteringpoints_testdata.name
  storage_account_name = azurerm_storage_account.this.name
}

resource "azurerm_storage_queue" "timeseries" {
  name                 = azurerm_storage_container.domain_test_timeseries_testdata.name
  storage_account_name = azurerm_storage_account.this.name
}

#---- System Topic for all storage account events

resource "azurerm_eventgrid_system_topic" "this" {
  name                   = "egst-${azurerm_storage_account.this.name}"
  resource_group_name    = azurerm_resource_group.this.name
  location               = azurerm_resource_group.this.location
  source_arm_resource_id = azurerm_storage_account.this.id
  topic_type             = "Microsoft.Storage.StorageAccounts"
  identity {
    type = "SystemAssigned"
  }
}

#---- System topic event subscriptions

resource "azurerm_eventgrid_system_topic_event_subscription" "domain_test_meteringpoints_testdata" {
  name                 = "egsts-${azurerm_storage_queue.metering_point_history.name}"
  system_topic         = azurerm_eventgrid_system_topic.this.name
  resource_group_name  = azurerm_resource_group.this.name
  included_event_types = ["Microsoft.Storage.BlobCreated"]

  subject_filter {
    subject_begins_with = "/blobServices/default/containers/${azurerm_storage_container.domain_test_meteringpoints_testdata.name}"
  }

  storage_queue_endpoint {
    storage_account_id = azurerm_storage_account.this.id
    queue_name         = azurerm_storage_queue.metering_point_history.name
  }
  delivery_identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_eventgrid_system_topic_event_subscription" "domain_test_timeseries_testdata" {
  name                 = "egsts-${azurerm_storage_queue.timeseries.name}"
  system_topic         = azurerm_eventgrid_system_topic.this.name
  resource_group_name  = azurerm_resource_group.this.name
  included_event_types = ["Microsoft.Storage.BlobCreated"]

  subject_filter {
    subject_begins_with = "/blobServices/default/containers/${azurerm_storage_container.domain_test_timeseries_testdata.name}"
  }

  storage_queue_endpoint {
    storage_account_id = azurerm_storage_account.this.id
    queue_name         = azurerm_storage_queue.timeseries.name
  }
  delivery_identity {
    type = "SystemAssigned"
  }
}
