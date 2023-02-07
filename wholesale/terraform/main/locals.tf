locals {
  STORAGE_CONTAINER_NAME                                             = "wholesale"

  # Service Bus domain event subscriptions
  # The names are made shorter due to name length limit of 50 characters in Azure and the module eats up like 15 of the characters for convention based naming
  CREATE_SETTLEMENT_REPORTS_WHEN_COMPLETED_BATCH_SUBSCRIPTION_NAME   = "create-settlement-reports"
  PUBLISH_PROCESSES_COMPLETED_WHEN_COMPLETED_BATCH_SUBSCRIPTION_NAME = "publish-processes-completed"
  PUBLISH_PROCESSESCOMPLETEDINTEGRATIONEVENT_WHEN_PROCESSCOMPLETED_SUBSCRIPTION_NAME = "publish-proc-completed-integ"
  PUBLISH_BATCH_CREATED_EVENT_WHEN_BATCH_CREATED_SUBSCRIPTION_NAME   = "batch-created"

  # Database
  DB_CONNECTION_STRING                                               = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_wholesale.name};Persist Security Info=False;Authentication=Active Directory Managed Identity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  DB_CONNECTION_STRING_SQL_AUTH                                      = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_wholesale.name};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.mssql_data_admin_name.value};Password=${data.azurerm_key_vault_secret.mssql_data_admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=120;"
  TIME_ZONE                                                          = "Europe/Copenhagen"    

  # Domain event names
  BATCH_COMPLETED_EVENT_NAME                                         = "BatchCompleted"
  PROCESS_COMPLETED_EVENT_NAME                                       = "ProcessCompleted"
  BATCH_CREATED_EVENT_NAME                                           = "BatchCreated"
}
