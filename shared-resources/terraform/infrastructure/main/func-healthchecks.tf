module "func_healthchecks" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=function-app_8.3.0"

  name                                   = "healthchecks"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = azurerm_subnet.vnetintegrations.id
  private_endpoint_subnet_id             = azurerm_subnet.privateendpoints.id
  app_service_plan_id                    = module.webapp_service_plan.id
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  always_on                              = true
  dotnet_framework_version               = "v8.0"
  application_insights_connection_string = module.appi_shared.connection_string

  health_check_alert = length(module.monitor_action_group_shres) != 1 ? null : {
    enabled         = true
    action_group_id = module.monitor_action_group_shres[0].id
  }

  role_assignments = [
    {
      resource_id          = module.kv_shared.id
      role_definition_name = "Key Vault Secrets User"
    },
    {
      resource_id          = azurerm_servicebus_topic.domainrelay_integrationevent_received.id
      role_definition_name = "Azure Service Bus Data Receiver"
    }
  ]
  app_settings = {
    SHARED_KEYVAULT_NAME  = "${module.kv_shared.name}"
    SHARED_DATALAKE_NAME  = "@Microsoft.KeyVault(VaultName=${module.kv_shared.name};SecretName=st-data-lake-name)"
    SERVICEBUS_ENDPOINT   = "${module.sb_domain_relay.endpoint}"
    SERVICEBUS_TOPIC_NAME = "@Microsoft.KeyVault(VaultName=${module.kv_shared.name};SecretName=sbt-shres-integrationevent-received-name)"
  }
}

resource "azurerm_role_assignment" "func_healthchecks_read_access_to_stdatalake" {
  scope                = module.st_data_lake.id
  role_definition_name = "Reader"
  principal_id         = module.func_healthchecks.identity.0.principal_id
}
