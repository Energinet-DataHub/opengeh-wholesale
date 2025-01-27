module "b2c_web_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/app-service?ref=app-service_7.1.0"

  name                       = "b2cwebapi"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  app_service_plan_id        = module.webapp_service_plan.id
  vnet_integration_subnet_id = data.azurerm_key_vault_secret.snet_vnetintegrations_id.value
  private_endpoint_subnet_id = data.azurerm_key_vault_secret.snet_privateendpoints_id.value
  dotnet_framework_version   = "v8.0"
  health_check_path          = "/monitor/ready"

  monitor_action_group = length(module.monitor_action_group_edi) != 1 ? null : {
    id                  = module.monitor_action_group_edi[0].id
    resource_group_name = azurerm_resource_group.this.name
  }

  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  application_insights_connection_string = data.azurerm_key_vault_secret.appi_shared_connection_string.value
  app_settings                           = local.b2c_web_api.app_settings

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
      // ServiceBus EDI Incomming Messages Queue
      resource_id          = azurerm_servicebus_queue.edi_incoming_messages_queue.id
      role_definition_name = "Azure Service Bus Data Receiver"
    },
    {
      // ServiceBus EDI Incomming Messages Queue
      resource_id          = azurerm_servicebus_queue.edi_incoming_messages_queue.id
      role_definition_name = "Azure Service Bus Data Sender"
    },
    {
      // ServiceBus Process Manager Topic
      resource_id          = data.azurerm_key_vault_secret.sbt_processmanager_id.value
      role_definition_name = "Azure Service Bus Data Owner"
    },
    {
      resource_id          = module.st_docs.id
      role_definition_name = "Storage Blob Data Contributor"
    }
  ]
}

module "kvs_app_edi_b2cwebapi_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "app-edi-b2cwebapi-base-url"
  value        = "https://${module.b2c_web_api.default_hostname}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
