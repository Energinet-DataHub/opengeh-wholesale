resource "azurerm_load_test" "this" {
  count               = var.create_azure_load_testing_resource ? 1 : 0

  location            = azurerm_resource_group.this.location
  name                = "lt-${var.domain_name_short}-${var.environment_short}-we-${var.environment_instance}"
  resource_group_name = azurerm_resource_group.this.name

  tags = local.tags
}
