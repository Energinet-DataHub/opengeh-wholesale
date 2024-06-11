resource "azurerm_resource_group" "this" {
  tags = merge({ "project_name" = "tftest" }, local.tags)
}
