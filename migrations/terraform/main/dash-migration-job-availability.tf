resource "azurerm_portal_dashboard" "migration_availability_dashboard" {
  name                = "Migration-availability"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  tags = {
    hidden-title = "Migration availability (${lower(var.environment_short)}-${var.environment_instance})"
  }
  dashboard_properties = templatefile("dashboard-templates/migration_job_availability.tpl",
    {
      subscription_id                         = data.azurerm_subscription.this.subscription_id,
      shared_resources_resource_group_name    = var.shared_resources_resource_group_name,
      migration_resources_resource_group_name = azurerm_resource_group.this.name,
      shared_storage_name                     = data.azurerm_key_vault_secret.st_data_lake_name.value,
      migration_storage_name                  = module.st_migrations.name
  })
}
