data "azuread_group" "pim_contributor_group" {
  count        = var.pim_contributor_group_name != null ? 1 : 0
  display_name = var.pim_contributor_group_name
}

resource "azurerm_role_assignment" "pim_contributor_contributor" {
  count = var.pim_contributor_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Contributor"
  principal_id         = data.azuread_group.pim_contributor_group[0].object_id
}

resource "azurerm_role_assignment" "pim_contributor_storage_blob_data_contributor" {
  count = var.pim_contributor_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azuread_group.pim_contributor_group[0].object_id
}

resource "azurerm_role_assignment" "pim_contributor_storage_queue_data_contributor" {
  count = var.pim_contributor_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Storage Queue Data Contributor"
  principal_id         = data.azuread_group.pim_contributor_group[0].object_id
}

resource "azurerm_role_assignment" "pim_contributor_service_bus_data_owner" {
  count = var.pim_contributor_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Azure Service Bus Data Owner"
  principal_id         = data.azuread_group.pim_contributor_group[0].object_id
}

resource "azurerm_role_assignment" "pim_contributor_key_vault_administrator" {
  count = var.pim_contributor_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azuread_group.pim_contributor_group[0].object_id
}

resource "azurerm_role_assignment" "pim_contributor_eventhubs_data_receiver" {
  count = var.pim_contributor_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Azure Event Hubs Data Receiver"
  principal_id         = data.azuread_group.pim_contributor_group[0].object_id
}

resource "azurerm_role_assignment" "pim_contributor_backup_contributor" {
  count = var.pim_contributor_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Backup Contributor"
  principal_id         = data.azuread_group.pim_contributor_group[0].object_id
}

resource "azurerm_role_assignment" "pim_contributor_locks_contributor" {
  count = var.pim_contributor_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = azurerm_role_definition.locks_contributor_access.name
  principal_id         = data.azuread_group.pim_contributor_group[0].object_id
}
