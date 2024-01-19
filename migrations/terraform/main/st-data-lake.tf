module "st_migrations" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account-dfs?ref=v13"

  name                            = "migrations"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  account_replication_type        = "LRS"
  account_tier                    = "Standard"
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  ip_rules                        = local.ip_restrictions_as_string
  prevent_deletion                = true
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

resource "azurerm_storage_container" "schema_migration" {
  name                  = "schema-migration"
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
  principal_id         = azuread_service_principal.spn_databricks.id
}

resource "azurerm_role_assignment" "ra_migrations_queue_data_contributor" {
  scope                = module.st_migrations.id
  role_definition_name = "Storage Queue Data Contributor"
  principal_id         = azuread_service_principal.spn_databricks.id
}

resource "azurerm_role_assignment" "ra_migrations_queue_data_reader" {
  scope                = module.st_migrations.id
  role_definition_name = "Storage Queue Data Reader"
  principal_id         = azuread_service_principal.spn_databricks.id
}

resource "azurerm_role_assignment" "st_migrations_queue_data_sender" {
  scope                = module.st_migrations.id
  role_definition_name = "Storage Queue Data Message Sender"
  principal_id         = azurerm_eventgrid_system_topic.st_migrations.identity[0].principal_id
}


