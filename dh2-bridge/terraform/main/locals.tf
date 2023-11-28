locals {
  DH2_BRIDGE_DOCUMENT_STORAGE_CONTAINER_NAME  = "grid-loss-documents"
  DH2_BRIDGE_DOCUMENT_STORAGE_ACCOUNT_URI     = "https://${module.storage_dh2_bridge_documents.name}.blob.core.windows.net"
  MS_DH2_BRIDGE_CONNECTION_STRING             = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_dh2_bridge.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  CONNECTION_STRING_DB_MIGRATIONS             = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_dh2_bridge.name};Persist Security Info=False;Authentication=Active Directory Default;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  DH2_ENDPOINT                                = "https://${module.app_dh2_placeholder.default_hostname}"
}
