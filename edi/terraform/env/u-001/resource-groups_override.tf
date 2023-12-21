resource "azurerm_resource_group" "this" {
  name = "rg-DataHub-edi-U-001"
}

data "azurerm_resource_group" "shared" {
  name = "rg-DataHub-SharedResouces-U-001"
}
