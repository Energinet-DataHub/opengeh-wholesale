data "azurerm_subscription" "current" {
}

resource "azurerm_role_assignment" "platformteam_subscription_contributor" {
  scope                = data.azurerm_subscription.current.id
  role_definition_name = "Contributor"
  principal_id         = "0524342a-e770-4b60-8d8d-d639d688b5b9"  # SEC-A-GreenForce-PlatformTeamAzure
}

resource "azurerm_role_assignment" "migrations_resourcegroup_contributor" {
  scope                = azurerm_resource_group.this.id
  role_definition_name = "Contributor"
  principal_id         = "74c4f45a-a295-47af-b665-b60eecba55d3"  # SEC-A-GreenForce-MigrationsTeamAzure
}

resource "azurerm_role_assignment" "migrations_storagereader" {
  scope                = module.st_dh2data.id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = "74c4f45a-a295-47af-b665-b60eecba55d3"  # SEC-A-GreenForce-MigrationsTeamAzure
}

