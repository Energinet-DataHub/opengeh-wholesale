locals {
  # Databricks runtime version for settlement report job
  # Python version for "15.4.x-scala2.12" is 3.11.0
  spark_version = "15.4.x-scala2.12"

  DB_CONNECTION_STRING      = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_settlement_report.name};Persist Security Info=False;Authentication=Active Directory Default;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  NAME_SUFFIX               = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  ip_restrictions_as_string = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])
  ENV_DESC                  = "${var.environment}_${var.environment_instance}"

  # Storage (Blob)
  BLOB_STORAGE_ACCOUNT_URI                   = "https://${module.storage_settlement_reports.name}.blob.core.windows.net"
  BLOB_STORAGE_ACCOUNT_JOBS_URI              = "https://${data.azurerm_key_vault_secret.st_settlement_report_name.value}.blob.core.windows.net"
  BLOB_CONTAINER_SETTLEMENTREPORTS_NAME      = "settlement-reports"
  BLOB_CONTAINER_JOBS_SETTLEMENTREPORTS_NAME = "settlement-reports"

  # Database
  TIME_ZONE = "Europe/Copenhagen"

  tags = {
    "BusinessServiceName"   = "Datahub",
    "BusinessServiceNumber" = "BSN10136"
  }
}
