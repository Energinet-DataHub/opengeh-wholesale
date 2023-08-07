locals {
  # Storage
  STORAGE_CONTAINER_NAME = "wholesale"
  STORAGE_ACCOUNT_URI = "https://${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net"

  # Database
  DB_CONNECTION_STRING          = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_wholesale.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  DB_CONNECTION_STRING_SQL_AUTH = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_wholesale.name};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.mssql_data_admin_name.value};Password=${data.azurerm_key_vault_secret.mssql_data_admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  TIME_ZONE                     = "Europe/Copenhagen"

  # Logging
  LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT                     = "Warning"
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_WHOLESALE = "Information"
}
