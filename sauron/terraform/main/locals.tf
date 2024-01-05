locals {
  connection_string_database_db = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb.name};Persist Security Info=False;Authentication=Active Directory Default;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  name_suffix                   = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  frontend_url                  = var.frontend_url != null ? var.frontend_url : azurerm_static_site.this.default_host_name
}
