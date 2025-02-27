module "apima_health_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api?ref=api-management-api_7.0.0"

  name                       = "sauron-health-checks-api"
  project_name               = var.domain_name_short
  api_management_name        = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name        = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  display_name               = "Sauron Health Checks Api"
  authorization_server_name  = data.azurerm_key_vault_secret.apim_oauth_server_name.value
  apim_logger_id             = data.azurerm_key_vault_secret.apim_logger_id.value
  logger_sampling_percentage = 100.0
  logger_verbosity           = "error"
  backend_service_url        = "https://${module.func_healthchecks.default_hostname}"
  path                       = "healthchecks"
  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <set-header name="RequestTime" exists-action="override">
                <value>@(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))</value>
            </set-header>
            <cors allow-credentials="true">
                <allowed-origins>
                    <origin>https://sauron.datahub3.dk</origin>
                    <origin>https://sauron.test002.datahub3.dk</origin>
                    <origin>https://localhost:3000</origin>
                </allowed-origins>
                <allowed-methods preflight-result-max-age="300">
                    <method>*</method>
                </allowed-methods>
                <allowed-headers>
                    <header>*</header>
                </allowed-headers>
                <expose-headers>
                    <header>*</header>
                </expose-headers>
            </cors>
          </inbound>
          <backend>
              <base />
          </backend>
          <outbound>
              <base />
          </outbound>
          <on-error>
              <base />
          </on-error>
        </policies>
      XML
    }
  ]
}

module "apimao_health_checks_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=api-management-api-operation_7.0.0"

  operation_id            = "healthchecks-api"
  api_management_api_name = module.apima_health_api.name
  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  display_name            = "Health Checks: API"
  method                  = "GET"
  url_template            = "/api/getHealthChecks/{env}"
  template_parameters = [
    {
      name     = "env"
      required = true
      type     = "string"
    }
  ]
}
