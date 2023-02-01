locals {
  CONNECTION_STRING = "Server=tcp:${azurerm_mssql_server.performance_test.fully_qualified_domain_name},1433;Initial Catalog=${module.mssqldb_edi_performance_test.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}