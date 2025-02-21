module "func_healthchecks_container" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/container-function-app?ref=container-function-app_1.3.0"

  name                                   = "healthchecks"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = azurerm_subnet.vnetintegrations.id
  private_endpoint_subnet_id             = azurerm_subnet.privateendpoints.id
  service_plan_id                        = module.webapp_service_plan.id
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  always_on                              = true
  application_insights_connection_string = module.appi_shared.connection_string
  docker_registry_server_username        = "PerTHenriksen"
  docker_registry_server_password        = local.pat_token_secret
  docker_registry_server_url             = "https://ghcr.io"
  health_check_path                      = "/api/monitor/ready"

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

resource "azurerm_role_assignment" "func_healthchecks_read_access_to_stdatalake_container" {
  scope                = module.st_data_lake.id
  role_definition_name = "Reader"
  principal_id         = module.func_healthchecks_container.identity.0.principal_id
}
