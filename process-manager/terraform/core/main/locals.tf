locals {
  # Logging
  LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT                           = "Information" # Everything else
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE            = "Information" # From geh-core
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_PROCESS_MANAGER = "Information" # From opengeh-process-manager

  OrchestrationsTaskHubName = "ProcessManager03"
  DatabaseConnectionString  = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_process_manager.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

  # Outlaw stuff
  tags = {
    "BusinessServiceName"   = "Datahub",
    "BusinessServiceNumber" = "BSN10136"
  }

  # IP Whitelist
  ip_restrictions_as_string = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])
}
