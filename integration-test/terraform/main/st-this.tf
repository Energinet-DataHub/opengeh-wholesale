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
