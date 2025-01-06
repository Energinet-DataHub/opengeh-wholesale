module "func_receiver" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app-elastic-durable?ref=function-app-elastic-durable_5.3.0"

  name                                  = "api"
  project_name                          = var.domain_name_short
  environment_short                     = var.environment_short
  environment_instance                  = var.environment_instance
  resource_group_name                   = azurerm_resource_group.this.name
  location                              = azurerm_resource_group.this.location
  app_service_plan_id                   = module.func_service_plan.id
  vnet_integration_subnet_id = data.azurerm_key_vault_secret.snet_vnetintegrations_id.value
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  allowed_monitor_reader_entra_groups   = compact([var.developer_security_group_name, var.pim_reader_group_name])
  durabletask_storage_connection_string = "See app setting 'OrchestrationsStorageConnectionString'"
  dotnet_framework_version              = "v8.0"
  use_dotnet_isolated_runtime           = true
  health_check_path                     = "/api/monitor/ready"

  health_check_alert = length(module.monitor_action_group_edi) != 1 ? null : {
    action_group_id = module.monitor_action_group_edi[0].id
    enabled         = true
  }

  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  client_certificate_mode                = "Optional"
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  app_settings                           = local.func_receiver.app_settings

  # Role assigments is needed to connect to the storage account (st_documents) using URI
  role_assignments = [
    {
      resource_id          = module.st_documents.id
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      resource_id          = data.azurerm_key_vault.kv_shared_resources.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      // ServiceBus Integration Events Topic
      resource_id          = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_id.value
      role_definition_name = "Azure Service Bus Data Owner"
    },
    {
      // ServiceBus Wholesale Inbox Queue
      resource_id          = data.azurerm_key_vault_secret.sbq_wholesale_inbox_id.value
      role_definition_name = "Azure Service Bus Data Owner"
    },
    {
      // ServiceBus EDI Inbox Queue
      resource_id          = data.azurerm_key_vault_secret.sbq_edi_inbox_id.value
      role_definition_name = "Azure Service Bus Data Owner"
    },
    {
      // ServiceBus EDI Incomming Messages Queue
      resource_id          = azurerm_servicebus_queue.edi_incoming_messages_queue.id
      role_definition_name = "Azure Service Bus Data Owner"
    },
    {
      // Dead-letter logs
      resource_id          = data.azurerm_key_vault_secret.st_deadltr_shres_id.value
      role_definition_name = "Storage Blob Data Contributor"
    },
    {
      // ServiceBus Process Manager Topic
      resource_id          = data.azurerm_key_vault_secret.sbt_processmanager_id.value
      role_definition_name = "Azure Service Bus Data Owner"
    },
    {
      // ServiceBus EDI Topic
      resource_id          = data.azurerm_key_vault_secret.sbt_edi_id.value
      role_definition_name = "Azure Service Bus Data Owner"
    },
  ]
}

module "kvs_edi_api_taskhub_storage_connection_string" {
  source       = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"
  name         = "func-edi-api-taskhub-storage-connection-string"
  value        = module.taskhub_storage_account.primary_connection_string
  key_vault_id = module.kv_internal.id
}
