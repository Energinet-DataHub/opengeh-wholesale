module "apima_b2b" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api?ref=v13"

  name                       = "b2b"
  project_name               = var.domain_name_short
  api_management_name        = data.azurerm_key_vault_secret.apim_instance_name.value
  resource_group_name        = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  display_name               = "B2B Api"
  authorization_server_name  = data.azurerm_key_vault_secret.apim_oauth_server_name.value
  apim_logger_id             = data.azurerm_key_vault_secret.apim_logger_id.value
  logger_sampling_percentage = 100.0
  logger_verbosity           = "verbose"
  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <trace source="B2B API" severity="verbose">
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
                                callerId = (jwt.Claims.GetValueOrDefault("azp", "(empty)"));
                            }
                        }
                    }
                    return $"Caller ID (claims.azp): {callerId}";
                }</message>
                <metadata name="CorrelationId" value="@($"{context.RequestId}")" />
            </trace>
            <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized. Failed policy requirements, or token is invalid or missing.">
                <openid-config url="https://login.microsoftonline.com/${data.azurerm_key_vault_secret.apim_b2c_tenant_id.value}/v2.0/.well-known/openid-configuration" />
                <required-claims>
                    <claim name="aud" match="any">
                        <value>${data.azurerm_key_vault_secret.apim_b2b_app_id.value}</value>
                    </claim>
                </required-claims>
            </validate-jwt>
            <base />
            <choose>
                <when condition="@(context.Request.Method == "POST")">
                    <check-header name="Content-Type" failed-check-httpcode="415" failed-check-error-message="Content-Type must be either application/xml or application/json" ignore-case="true">
                      <value>application/xml</value>
                      <value>application/xml; charset=utf-8</value>
                      <value>application/json</value>
                    </check-header>
                    <set-variable name="bodySize" value="@(context.Request.Headers["Content-Length"][0])" />
                    <choose>
                        <when condition="@(int.Parse(context.Variables.GetValueOrDefault<string>("bodySize"))<52428800)">
                            <!--let it pass through by doing nothing-->
                        </when>
                        <otherwise>
                            <return-response>
                                <set-status code="413" reason="Payload Too Large" />
                                <set-body>@{
                                        return "Maximum allowed size for the POST requests is 52428800 bytes (50 MB). This request has size of "+ context.Variables.GetValueOrDefault<string>("bodySize") +" bytes";
                                    }</set-body>
                            </return-response>
                        </otherwise>
                    </choose>
                </when>
            </choose>
            <set-header name="CorrelationId" exists-action="override">
                <value>@($"{context.RequestId}")</value>
            </set-header>
            <set-header name="RequestTime" exists-action="override">
                <value>@(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))</value>
            </set-header>
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
