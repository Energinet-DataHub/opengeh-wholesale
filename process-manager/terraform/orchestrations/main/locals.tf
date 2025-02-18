locals {
  # Wholesale database name
  wholesale_db_name = "mssqldb-data-wholsal-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"

  # Logging
  LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT                           = "Information" # Everything else
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE            = "Information" # From geh-core
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_PROCESS_MANAGER = "Information" # From opengeh-process-manager

  # Task Hub name must match PM Core locals OrchestrationsTaskHubName
  OrchestrationsTaskHubName = "ProcessManager05"

  # Outlaw stuff
  tags = {
    "BusinessServiceName"   = "Datahub",
    "BusinessServiceNumber" = "BSN10136"
  }

  # IP Whitelist
  ip_restrictions_as_string = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])
}
