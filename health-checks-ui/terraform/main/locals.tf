locals {
  mssql_connection_string = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_health_checks_ui.name};User ID=${data.azurerm_key_vault_secret.mssql_data_admin_name.value};Password=${data.azurerm_key_vault_secret.mssql_data_admin_password.value};Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
}
