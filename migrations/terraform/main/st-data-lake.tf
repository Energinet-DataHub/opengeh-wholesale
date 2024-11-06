module "st_migrations" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=storage-account-dfs_9.1.0"

  name                       = "migrations"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  account_replication_type   = "LRS"
  prevent_deletion           = false
  use_queue                  = true
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                   = local.ip_restrictions_as_string
  role_assignments = [
    {
      principal_id         = data.azurerm_key_vault_secret.shared_access_connector_principal_id.value
      role_definition_name = "Storage Blob Data Contributor"
    },
  ]
  audit_storage_account = var.enable_audit_logs ? {
    id = data.azurerm_key_vault_secret.st_audit_shres_id.value
  } : null
}

module "kvs_st_migrations_data_lake_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "st-migrations-data-lake-name"
  value        = module.st_migrations.name
  key_vault_id = module.kv_internal.id
}

data "azurerm_key_vault_secret" "shared_access_connector_principal_id" {
  name         = "shared-access-connector-principal-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

#---- System Topic for all storage account events

resource "azurerm_eventgrid_system_topic" "st_migrations" {
  name                   = "egst-${module.st_migrations.name}-${local.resources_suffix}"
  resource_group_name    = azurerm_resource_group.this.name
  location               = azurerm_resource_group.this.location
  source_arm_resource_id = module.st_migrations.id
  topic_type             = "Microsoft.Storage.StorageAccounts"
  identity {
    type = "SystemAssigned"
  }

  tags = local.tags
}


#---- System topic event subscriptions

resource "azurerm_eventgrid_system_topic_event_subscription" "st_migrations_bronze" {
  name                 = "egsts-${azurerm_storage_queue.bronze.name}-${local.resources_suffix}"
  system_topic         = azurerm_eventgrid_system_topic.st_migrations.name
  resource_group_name  = azurerm_resource_group.this.name
  included_event_types = ["Microsoft.Storage.BlobCreated"]

  subject_filter {
    subject_begins_with = "/blobServices/default/containers/${azurerm_storage_container.bronze.name}"
  }

  storage_queue_endpoint {
    storage_account_id = module.st_migrations.id
    queue_name         = azurerm_storage_queue.bronze.name
  }
  delivery_identity {
    type = "SystemAssigned"
  }
}

#---- Containers

resource "azurerm_storage_container" "internal" {
  name                  = "internal"
  storage_account_name  = module.st_migrations.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "bronze" {
  name                  = "bronze"
  storage_account_name  = module.st_migrations.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "silver" {
  name                  = "silver"
  storage_account_name  = module.st_migrations.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "gold" {
  name                  = "gold"
  storage_account_name  = module.st_migrations.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "eloverblik" {
  name                  = "eloverblik"
  storage_account_name  = module.st_migrations.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "wholesale" {
  name                  = "wholesale"
  storage_account_name  = module.st_migrations.name
  container_access_type = "private"
}

#---- Queues

resource "azurerm_storage_queue" "bronze" {
  name                 = azurerm_storage_container.bronze.name
  storage_account_name = module.st_migrations.name
}

#---- Role assignments
resource "azurerm_role_assignment" "ra_migrations_contributor" {
  scope                = module.st_migrations.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.object_id
}

resource "azurerm_role_assignment" "ra_migrations_queue_data_contributor" {
  scope                = module.st_migrations.id
  role_definition_name = "Storage Queue Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.object_id
}

resource "azurerm_role_assignment" "ra_migrations_queue_data_reader" {
  scope                = module.st_migrations.id
  role_definition_name = "Storage Queue Data Reader"
  principal_id         = azuread_service_principal.spn_databricks.object_id
}

resource "azurerm_role_assignment" "st_migrations_queue_data_sender" {
  scope                = module.st_migrations.id
  role_definition_name = "Storage Queue Data Message Sender"
  principal_id         = azurerm_eventgrid_system_topic.st_migrations.identity[0].principal_id
}
