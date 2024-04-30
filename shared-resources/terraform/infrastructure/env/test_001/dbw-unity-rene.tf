resource "azurerm_databricks_workspace" "dbwrene" {
  name                        = "dbwrene-${local.resources_suffix}"
  resource_group_name         = azurerm_resource_group.this.name
  location                    = azurerm_resource_group.this.location
  sku                         = "premium"
  managed_resource_group_name = "rg-dbwrene-${local.resources_suffix}"
}
