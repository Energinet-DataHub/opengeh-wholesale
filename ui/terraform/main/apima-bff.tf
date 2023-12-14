module "apima_bff" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api?ref=v12"

  name                       = "bff"
  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  api_management_name        = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name        = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  display_name               = "BFF Api"
  authorization_server_name  = azurerm_api_management_authorization_server.oauth_server_bff.name
  apim_logger_id             = data.azurerm_key_vault_secret.apim_logger_id.value
  logger_sampling_percentage = 100.0
  logger_verbosity           = "verbose"
  path                       = "bff"
  backend_service_url        = "https://${module.bff.default_hostname}"
  import = {
    content_format = "openapi+json"
    content_value  = data.local_file.swagger_file.content
  }
  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <trace source="BFF API" severity="verbose">
                <message>@{
                    string authHeader = context.Request.Headers.GetValueOrDefault("Authorization", "");
                    string callerId = "(empty)";
                    if (authHeader?.Length > 0)
                    {
                        string[] authHeaderParts = authHeader.Split(' ');
                        if (authHeaderParts?.Length == 2 && authHeaderParts[0].Equals("Bearer", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Jwt jwt;
                            if (authHeaderParts[1].TryParseJwt(out jwt))
                            {
                                callerId = (jwt.Claims.GetValueOrDefault("sub", "(empty)"));
                            }
                        }
                    }
                    return $"Caller ID (claims.sub): {callerId}";
                }</message>
                <metadata name="CorrelationId" value="@($"{context.RequestId}")" />
            </trace>
            <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized. Failed policy requirements, or token is invalid or missing.">
                <openid-config url="${data.azurerm_key_vault_secret.frontend_open_id_url.value}" />
                <openid-config url="${data.azurerm_key_vault_secret.backend_open_id_url.value}" />
                <required-claims>
                    <claim name="aud" match="any">
                        <value>${data.azurerm_key_vault_secret.backend_bff_app_id.value}</value>
                    </claim>
                </required-claims>
            </validate-jwt>
            <base />
            <set-header name="CorrelationId" exists-action="override">
                <value>@($"{context.RequestId}")</value>
            </set-header>
            <set-header name="RequestTime" exists-action="override">
                <value>@(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))</value>
            </set-header>
            <cors allow-credentials="true">
                <allowed-origins>
                    <origin>https://${local.frontend_url}</origin>
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
              <set-header name="CorrelationId" exists-action="override">
                  <value>@($"{context.RequestId}")</value>
              </set-header>
          </outbound>
          <on-error>
              <base />
              <set-header name="CorrelationId" exists-action="override">
                  <value>@($"{context.RequestId}")</value>
              </set-header>
          </on-error>
        </policies>
      XML
    }
  ]
}

data "local_file" "swagger_file" {
  filename = "${path.module}/../../swagger.json"
}

resource "azurerm_api_management_authorization_server" "oauth_server_bff" {
  name                         = "bffoauthserver"
  api_management_name          = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name          = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  display_name                 = "BFF: OAuth client credentials server"
  client_registration_endpoint = "http://localhost/"
  grant_types = [
    "implicit",
  ]
  authorization_endpoint = local.b2c_authorization_sign_in_endpoint
  authorization_methods = [
    "GET",
  ]
  token_endpoint = local.b2c_authorization_token_endpoint
  client_authentication_method = [
    "Body",
  ]
  bearer_token_sending_methods = [
    "authorizationHeader",
  ]
  client_id = data.azurerm_key_vault_secret.backend_bff_app_id.value
}

module "kvs_app_bff_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v12"

  name         = "app-bff-base-url"
  value        = "${data.azurerm_key_vault_secret.apim_gateway_url.value}/${module.apima_bff.path}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
