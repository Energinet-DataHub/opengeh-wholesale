locals {
  ESETT_DOCUMENT_STORAGE_CONTAINER_NAME = "esett-documents"
  ESETT_DOCUMENT_STORAGE_ACCOUNT_URI    = "https://${module.storage_esett_documents.name}.blob.core.windows.net"

  DH2_CERTIFICATE_THUMBPRINT            = "none"
  DH2_ENDPOINT                          = "https://${module.app_dh2_placeholder.default_hostname}"

  MS_ESETT_EXCHANGE_CONNECTION_STRING   = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_esett_exchange.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  CONNECTION_STRING_DB_MIGRATIONS       = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_esett_exchange.name};Persist Security Info=False;Authentication=Active Directory Default;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"

  NAME_SUFFIX                           = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  ip_restrictions_as_string             = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])

  BIZ_TALK_CERTIFICATE_THUMBPRINT       = "none"
  BIZ_TALK_SENDER_CODE                  = ""
  BIZ_TALK_RECEIVER_CODE                = ""
  BIZ_TALK_BIZ_TALK_END_POINT           = "/EL_DataHubService/IntegrationService.svc"
  BIZ_TALK_BUSINESS_TYPE_CONSUMPTION    = "NBS-RECI"
  BIZ_TALK_BUSINESS_TYPE_PRODUCTION     = "NBS-MGXI"
  BIZ_TALK_BUSINESS_TYPE_EXCHANGE       = "NBS-MEPI"
}
