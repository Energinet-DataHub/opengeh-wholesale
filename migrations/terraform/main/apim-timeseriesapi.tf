module "apim_timeseriesapi" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api?ref=v12"

  name                       = "timeseriesapi"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  api_management_name        = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name        = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  authorization_server_name  = data.azurerm_key_vault_secret.apim_oauth_server_name.value
  display_name               = jsondecode(data.local_file.swagger_file.content).info.title
  description                = jsondecode(data.local_file.swagger_file.content).info.description
  apim_logger_id             = data.azurerm_key_vault_secret.apim_logger_id.value
  logger_sampling_percentage = 100.0
  logger_verbosity           = "verbose"
  path                       = "timeseriesapi"
  backend_service_url        = "https://${module.app_time_series_api.default_hostname}"
  import = {
    content_format = "openapi+json"
    content_value  = data.local_file.swagger_file.content
  }
  policies = [
    {
      xml_content = <<XML
                <policies>
                    <inbound>
                        <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized. Failed policy requirements, or token is invalid or missing.">
                            <openid-config url="https://login.microsoftonline.com/${data.azurerm_key_vault_secret.apim_b2c_tenant_id.value}/v2.0/.well-known/openid-configuration" />
                            <required-claims>
                                <claim name="aud" match="any">
                                    <value>${data.azurerm_key_vault_secret.apim_timeseriesapi_app_id.value}</value>
                                </claim>
                                <claim name="roles" match="all">
                                    <value>eloverblik</value>
                                </claim>
                            </required-claims>
                        </validate-jwt>
                        <base />
                    </inbound>
                    <backend>
                        <base />
                    </backend>
                    <outbound>
                        <base />
                        <set-header name="Authorization" exists-action="append">
                            <value>@(context.Request.Headers.GetValueOrDefault("Authorization"))</value>
                        </set-header>
                    </outbound>
                    <on-error>
                        <base />
                    </on-error>
                </policies>
            XML
    }
  ]
}

data "local_file" "swagger_file" {
  filename = "${path.module}/../../swagger.json"
}
