resource "azurerm_resource_group" "this" {
  name = "rg-DataHub-edi-T-001"
}

data "azurerm_resource_group" "shared" {
  name = "rg-DataHub-SharedResouces-T-001"
}
