locals {
  ESETT_DOCUMENT_STORAGE_CONTAINER_NAME        = "esett-documents"
  ESETT_DOCUMENT_STORAGE_ACCOUNT_URI           = "https://${module.storage_esett_documents.name}.blob.core.windows.net"
  ESETT_CERTIFICATE_THUMBPRINT                 = "none"
  DH2_ENDPOINT                                 = "https://${module.app_dh2_placeholder.default_hostname}"
  MS_ESETT_EXCHANGE_CONNECTION_STRING          = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_esett_exchange.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  MS_ESETT_EXCHANGE_CONNECTION_STRING_SQL_AUTH = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_esett_exchange.name};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.mssql_data_admin_name.value};Password=${data.azurerm_key_vault_secret.mssql_data_admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
}
