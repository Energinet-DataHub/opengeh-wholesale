locals {
  # Storage
  STORAGE_CONTAINER_NAME = "wholesale"
  STORAGE_ACCOUNT_URI    = "https://${data.azurerm_key_vault_secret.st_shared_data_lake_name.value}.dfs.core.windows.net"

  # Database
  DB_CONNECTION_STRING            = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_wholesale.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  CONNECTION_STRING_DB_MIGRATIONS = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_wholesale.name};Persist Security Info=False;Authentication=Active Directory Default;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  TIME_ZONE                       = "Europe/Copenhagen"

  # Logging
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_WHOLESALE = "Information" # From opengeh-wholesale
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE      = "Information" # From geh-core
  LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT                     = "Warning"     # Everything else

  # Service Bus
  INTEGRATIONEVENTS_SUBSCRIPTION_NAME = "integration-event"

  # IP restrictions
  ip_restrictions_as_string = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])
}
