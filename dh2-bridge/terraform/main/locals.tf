locals {
  DH2_BRIDGE_DOCUMENT_STORAGE_CONTAINER_NAME = "grid-loss-documents"
  DH2_BRIDGE_DOCUMENT_STORAGE_ACCOUNT_URI    = "https://${module.storage_dh2_bridge_documents.name}.blob.core.windows.net"
  MS_DH2_BRIDGE_CONNECTION_STRING            = "Server=tcp:${module.mssqldb_dh2_bridge.server_fqdn},1433;Initial Catalog=${module.mssqldb_dh2_bridge.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  DH2BRIDGE_CERTIFICATE_THUMBPRINT           = "none"
  ip_restrictions_as_string                  = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])

  tags = {
    "BusinessServiceName"   = "Datahub",
    "BusinessServiceNumber" = "BSN10136"
  }
}
