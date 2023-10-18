locals {
  CONNECTION_STRING          = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_edi.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  CONNECTION_STRING_SQL_AUTH = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_edi.name};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.mssql_data_admin_name.value};Password=${data.azurerm_key_vault_secret.mssql_data_admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  # Note: The following timezone name is using the naming scheme of the TZ Database. See https://en.wikipedia.org/wiki/List_of_tz_database_time_zones for list of possible values.
  TIME_ZONE = "Europe/Copenhagen"

  # Integration event subscription details
  INTEGRATION_EVENTS_TOPIC_NAME                       = "@Microsoft.KeyVault(VaultName=${var.shared_resources_keyvault_name};SecretName=sbt-shres-integrationevent-received-name)"
  WHOLESALE_PROCESS_COMPLETED_EVENT_SUBSCRIPTION_NAME = "balance-fixing-completed"
}
