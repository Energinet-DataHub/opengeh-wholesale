module "apima_bff_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api?ref=api-management-api_7.0.0"
  count  = 1

  name                       = "sauron-bff"
  project_name               = var.domain_name_short
  api_management_name        = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name        = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  display_name               = "Sauron BFF Api"
  authorization_server_name  = data.azurerm_key_vault_secret.apim_oauth_server_name.value
  apim_logger_id             = data.azurerm_key_vault_secret.apim_logger_id.value
  logger_sampling_percentage = 100.0
  logger_verbosity           = "verbose"
  backend_service_url        = "https://${module.func_bff_api.default_hostname}"
  path                       = "sauron"
}

module "apimao_get_deployments" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=api-management-api-operation_7.0.0"

  count                   = 1
  operation_id            = "get-deployments"
  api_management_api_name = module.apima_bff_api[0].name
  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  display_name            = "Sauron: GET deployments"
  method                  = "GET"
  url_template            = "/api/GetDeployments"
}

module "apimao_get_status" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=api-management-api-operation_7.0.0"

  count                   = 1
  operation_id            = "get-status"
  api_management_api_name = module.apima_bff_api[0].name
  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  display_name            = "Sauron: GET status"
  method                  = "GET"
  url_template            = "/api/GetStatus"
}
