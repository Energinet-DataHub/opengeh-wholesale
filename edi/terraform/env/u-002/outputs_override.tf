output ms_edi_database_name {
  description = "Database name in the sql server for performance testing"
  value = module.mssqldb_edi_performance_test.name
  sensitive = true  
}

output ms_edi_database_server {
  description = "Database server instance hosting the EDI database for performance testing"
  value = azurerm_mssql_server.performance_test.fully_qualified_domain_name
  sensitive = true  
}