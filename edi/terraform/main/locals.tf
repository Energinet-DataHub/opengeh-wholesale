locals {
  CONNECTION_STRING               = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_edi.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  CONNECTION_STRING_DB_MIGRATIONS = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_edi.name};Persist Security Info=False;Authentication=Active Directory Default;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

  # Integration event subscription details
  INTEGRATION_EVENTS_TOPIC_NAME        = "@Microsoft.KeyVault(VaultName=${data.azurerm_key_vault.kv_shared_resources.name};SecretName=sbt-shres-integrationevent-received-name)"
  INTEGRATION_EVENTS_SUBSCRIPTION_NAME = "integration-event"

  # IP Whitelist
  ip_restrictions_as_string = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])

  # Data lake
  AZURE_STORAGE_ACCOUNT_URL = "https://${module.st_documents.name}.blob.core.windows.net"

  # Logging
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_EDI  = "Information" # From opengeh-edi
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE = "Information" # From geh-core
  LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT                = "Information" # Everything else

  tags = {
    "BusinessServiceName"   = "Datahub",
    "BusinessServiceNumber" = "BSN10136"
  }

  OrchestrationsTaskHubName = "Edi01"
}
