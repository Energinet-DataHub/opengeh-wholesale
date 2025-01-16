locals {
  MS_ELECTRICITY_MARKET_CONNECTION_STRING = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_electricity_market.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  MS_MARKPART_DB_CONNECTION_STRING        = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${data.azurerm_key_vault_secret.markpart_db_name.value};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  CONNECTION_STRING_DB_MIGRATIONS         = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${data.azurerm_key_vault_secret.markpart_db_name.value};Persist Security Info=False;Authentication=Active Directory Default;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  NAME_SUFFIX                             = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  ip_restrictions_as_string               = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])
  ENV_DESC                                = "${var.environment}_${var.environment_instance}"

  # Database
  TIME_ZONE = "Europe/Copenhagen"

  tags = {
    "BusinessServiceName"   = "Datahub",
    "BusinessServiceNumber" = "BSN10136"
  }
}
