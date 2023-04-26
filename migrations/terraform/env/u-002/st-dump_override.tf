data "azurerm_resource_group" "migrations" {
  #Name in U-002 differs from all other environments
  name = "rg-DataHub-DataMigration-${upper(var.environment_short)}-${var.environment_instance}"
}
data "azurerm_storage_account" "drop" {
  #Name in U-002 differs from all other environments
  name                = "sttsdropdatmigendk${lower(var.environment_short)}${var.environment_instance}"
  resource_group_name = data.azurerm_resource_group.migrations.name
}
