locals {
  mssql_connection_string      = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_health_checks_ui.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  resource_suffix_with_dash    = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  resource_suffix_without_dash = "${lower(var.domain_name_short)}${lower(var.environment_short)}we${lower(var.environment_instance)}"
}
