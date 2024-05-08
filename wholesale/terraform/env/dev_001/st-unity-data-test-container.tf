# The storage containers are not created in the module, as they are used in schema creation. I.e., we want it dynamically
resource "azurerm_storage_container" "migration_wholesale_test" {
  name                 = "migration-wholesale-test"
  storage_account_name = module.st_data_wholesale.name
}
