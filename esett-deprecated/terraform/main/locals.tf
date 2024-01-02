// Local Variables
locals {
  blob_files_raw_container_name             = "files-raw"
  blob_files_enrichments_container_name     = "files-enrichments"
  blob_files_converted_container_name       = "files-converted"
  blob_files_sent_container_name            = "files-sent"
  blob_files_error_container_name           = "files-error"
  blob_files_confirmed_container_name       = "files-confirmed"
  blob_files_other_container_name           = "files-other"
  blob_files_mga_imbalance_container_name   = "files-mga-imbalance"
  blob_files_brp_change_container_name      = "files-brp-change"
  blob_files_ack_container_name             = "files-acknowledgement"
  name_suffix                               = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  name_suffix_no_dash                       = "${lower(var.domain_name_short)}${lower(var.environment_short)}we${lower(var.environment_instance)}"
  connection_string_database                = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_esett.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  connection_string_database_db_migrations  = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_esett.name};Persist Security Info=False;Authentication=Active Directory Default;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  ip_restrictions_as_string                 = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])
}
