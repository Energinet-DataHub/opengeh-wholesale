locals {
  CONNECTION_STRING               = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_edi.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  CONNECTION_STRING_DB_MIGRATIONS = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_edi.name};Persist Security Info=False;Authentication=Active Directory Default;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  # Note: The following timezone name is using the naming scheme of the TZ Database. See https://en.wikipedia.org/wiki/List_of_tz_database_time_zones for list of possible values.
  TIME_ZONE = "Europe/Copenhagen"

  # Integration event subscription details
  INTEGRATION_EVENTS_TOPIC_NAME        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
  INTEGRATION_EVENTS_SUBSCRIPTION_NAME = "integration-event"
}
