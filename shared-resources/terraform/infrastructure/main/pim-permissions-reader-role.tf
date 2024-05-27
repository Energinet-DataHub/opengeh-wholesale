data "azuread_group" "pim_reader_group" {
  count        = var.pim_reader_group_name != null ? 1 : 0
  display_name = var.pim_reader_group_name
}

resource "azurerm_role_assignment" "pim_reader_reader" {
  count = var.pim_reader_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Reader"
  principal_id         = data.azuread_group.pim_reader_group[0].object_id
}

resource "azurerm_role_assignment" "pim_reader_storage_blob_data_reader" {
  count = var.pim_reader_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = data.azuread_group.pim_reader_group[0].object_id
}

resource "azurerm_role_assignment" "pim_reader_storage_queue_data_reader" {
  count = var.pim_reader_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Storage Queue Data Reader"
  principal_id         = data.azuread_group.pim_reader_group[0].object_id
}

resource "azurerm_role_assignment" "pim_reader_service_bus_data_receiver" {
  count = var.pim_reader_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Azure Service Bus Data Receiver"
  principal_id         = data.azuread_group.pim_reader_group[0].object_id
}

resource "azurerm_role_assignment" "pim_reader_key_vault_certificate_user" {
  count = var.pim_reader_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Certificate User"
  principal_id         = data.azuread_group.pim_reader_group[0].object_id
}

resource "azurerm_role_assignment" "pim_reader_key_vault_secrets_user" {
  count = var.pim_reader_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = data.azuread_group.pim_reader_group[0].object_id
}

resource "azurerm_role_assignment" "pim_reader_key_vault_crypto_user" {
  count = var.pim_reader_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Crypto User"
  principal_id         = data.azuread_group.pim_reader_group[0].object_id
}

resource "azurerm_role_assignment" "pim_reader_key_vault_reader" {
  count = var.pim_reader_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Key Vault Reader"
  principal_id         = data.azuread_group.pim_reader_group[0].object_id
}

resource "azurerm_role_assignment" "pim_reader_eventhubs_data_receiver" {
  count = var.pim_reader_group_name != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Azure Event Hubs Data Receiver"
  principal_id         = data.azuread_group.pim_reader_group[0].object_id
}
