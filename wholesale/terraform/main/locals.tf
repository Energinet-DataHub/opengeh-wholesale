locals {
  # Databricks runtime version for migration and calculation jobs
  # Python version for "15.4.x-scala2.12" is 3.11.0
  spark_version = "15.4.x-scala2.12"
  spark_version_with_photon = "15.4.x-photon-scala2.12"

  # Storage (DataLake)
  STORAGE_CONTAINER_NAME = "wholesale"
  STORAGE_ACCOUNT_URI    = "https://${data.azurerm_key_vault_secret.st_data_lake_name.value}.dfs.core.windows.net"

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

  resource_suffix_with_dash = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  tags = {
    "BusinessServiceName"   = "Datahub",
    "BusinessServiceNumber" = "BSN10136"
  }

  # Databricks permissions
  # Local readers determines if the provided reader security group should be assigned permissions or grants.
  # This is necessary as reader and contributor groups may be the same on the development and test environments. In Databricks, the grants and permissions of a security group can't be be managed by multiple resources.
  readers = var.databricks_readers_group.name == var.databricks_contributor_dataplane_group.name ? {} : { "${var.databricks_readers_group.name}" = "${var.databricks_readers_group.id}" }
}
