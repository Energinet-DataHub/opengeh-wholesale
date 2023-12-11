module "apima_bff_graphql_temp" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api?ref=v13"

  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <trace source="BFF GraphQL API" severity="verbose">
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
