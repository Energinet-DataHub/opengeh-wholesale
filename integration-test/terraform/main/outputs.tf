output "resource_group_name" {
  value = azurerm_resource_group.this.name
}

output "dbw_resource_group_name" {
  value = azurerm_databricks_workspace.this.managed_resource_group_id
}
