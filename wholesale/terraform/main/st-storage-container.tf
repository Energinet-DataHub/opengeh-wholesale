resource "azurerm_storage_container" "storage_container" {
  # The name is hardcoded in repo `opengeh-wholesale` as well
  name                 = "wholesale"
  storage_account_name = data.azurerm_key_vault_secret.st_data_lake_name.value
}
