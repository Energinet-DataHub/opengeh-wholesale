module "st_dh2timeseries_audit" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_8.0.0"

  name                       = "dh2tsaudit"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "LRS"
  prevent_deletion           = false
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                   = local.ip_restrictions_as_string
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null
}

module "kvs_st_dh2timeseries_audit" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_5.0.0"

  name         = "st-dh2timeseries-audit-name"
  value        = module.st_dh2timeseries_audit.name
  key_vault_id = module.kv_internal.id
}

#---- System Topic for all storage account events

resource "azurerm_eventgrid_system_topic" "st_dh2timeseries_audit" {
  name                   = "egst-${module.st_dh2timeseries_audit.name}-${local.resources_suffix}"
  resource_group_name    = azurerm_resource_group.this.name
  location               = azurerm_resource_group.this.location
  source_arm_resource_id = module.st_dh2timeseries_audit.id
  topic_type             = "Microsoft.Storage.StorageAccounts"
  identity {
    type = "SystemAssigned"
  }

  tags = local.tags
}

#---- System topic event subscriptions

resource "azurerm_eventgrid_system_topic_event_subscription" "dh2timeseries_audit" {
  name                 = "egsts-${azurerm_storage_queue.timeseries_audit.name}-${local.resources_suffix}"
  system_topic         = azurerm_eventgrid_system_topic.st_dh2timeseries_audit.name
  resource_group_name  = azurerm_resource_group.this.name
  included_event_types = ["Microsoft.Storage.BlobCreated"]

  subject_filter {
    subject_begins_with = "/blobServices/default/containers/${azurerm_storage_container.timeseriesaudit.name}/"
  }

  storage_queue_endpoint {
    storage_account_id = module.st_dh2timeseries_audit.id
    queue_name         = azurerm_storage_queue.timeseries_audit.name
  }
  delivery_identity {
    type = "SystemAssigned"
  }

  depends_on = [azurerm_role_assignment.st_dh2timeseries_audit_queue_data_sender]
}

#---- Queues

resource "azurerm_storage_queue" "timeseries_audit" {
  name                 = azurerm_storage_container.timeseriesaudit.name
  storage_account_name = module.st_dh2timeseries_audit.name
}

#---- Role assignments

resource "azurerm_role_assignment" "ra_dh2timeseriesaudit_contributor" {
  scope                = module.st_dh2timeseries_audit.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.object_id
}

resource "azurerm_role_assignment" "ra_dh2timeseries_queue_data_contributor" {
  scope                = module.st_dh2timeseries_audit.id
  role_definition_name = "Storage Queue Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.object_id
}

resource "azurerm_role_assignment" "ra_dh2timeseries_queue_data_reader" {
  scope                = module.st_dh2timeseries_audit.id
  role_definition_name = "Storage Queue Data Reader"
  principal_id         = azuread_service_principal.spn_databricks.object_id
}

resource "azurerm_role_assignment" "st_dh2timeseries_audit_queue_data_sender" {
  scope                = module.st_dh2timeseries_audit.id
  role_definition_name = "Storage Queue Data Message Sender"
  principal_id         = azurerm_eventgrid_system_topic.st_dh2timeseries_audit.identity[0].principal_id
}


#---- Containers

resource "azurerm_storage_container" "timeseriesaudit" {
  name                  = "dh2-time-series-audit"
  storage_account_name  = module.st_dh2timeseries_audit.name
  container_access_type = "private"
}
