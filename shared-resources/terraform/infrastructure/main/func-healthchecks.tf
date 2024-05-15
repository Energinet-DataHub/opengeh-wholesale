module "func_healthchecks" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=14.0.3"

  name                                   = "healthchecks"
  project_name                           = var.domain_name_short
  environment_short                      = var.environment_short
  environment_instance                   = var.environment_instance
  resource_group_name                    = azurerm_resource_group.this.name
  location                               = azurerm_resource_group.this.location
  vnet_integration_subnet_id             = data.azurerm_subnet.snet_vnet_integration.id
  private_endpoint_subnet_id             = data.azurerm_subnet.snet_private_endpoints.id
  app_service_plan_id                    = module.webapp_service_plan.id
  ip_restrictions                        = var.ip_restrictions
  scm_ip_restrictions                    = var.ip_restrictions
  always_on                              = true
  dotnet_framework_version               = "v8.0"
  application_insights_connection_string = module.appi_shared.connection_string


  role_assignments = [
    {
      resource_id          = module.kv_shared.id
      role_definition_name = "Key Vault Secrets User"
    }
  ]
  app_settings = {
    SHARED_KEYVAULT_NAME         = "${module.kv_shared.name}"
    SHARED_DATALAKE_NAME         = "@Microsoft.KeyVault(VaultName=${module.kv_shared.name};SecretName=st-data-lake-name)"
    SERVICEBUS_CONNECTION_STRING = "@Microsoft.KeyVault(VaultName=${module.kv_shared.name};SecretName=sb-domain-relay-manage-connection-string)"
    SERVICEBUS_TOPIC_NAME        = "@Microsoft.KeyVault(VaultName=${module.kv_shared.name};SecretName=sbt-shres-integrationevent-received-name)"
  }
}

resource "azurerm_role_assignment" "func_healthchecks_read_access_to_stdatalake" {
  scope                = module.st_data_lake.id
  role_definition_name = "Reader"
  principal_id         = module.func_healthchecks.identity.0.principal_id
}
