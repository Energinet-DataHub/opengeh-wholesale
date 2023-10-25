module "apimao_ping_for_connectivity_test" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=v12"

  operation_id            = "connectivity-test-ping"
  api_management_api_name = module.apim_timeseriesapi.name
  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  display_name            = "Connectivity test: ping 200"
  method                  = "GET"
  url_template            = "/GetByMeteringPointIds?meteringPointIds=571313124410187086&dateFromEpoch=1640991600&dateToEpoch=1643670000"
  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <validate-jwt header-name="Authorization" failed-validation-httpcode="403" failed-validation-error-message="Unauthorized to access this endpoint.">
                <openid-config url="https://login.microsoftonline.com/${data.azurerm_key_vault_secret.apim_b2c_tenant_id.value}/v2.0/.well-known/openid-configuration" />
                <required-claims>
                    <claim name="aud">
                        <value>${data.azurerm_key_vault_secret.apim_timeseriesapi_app_id.value}</value>
                    </claim>
                    <claim name="roles" match="any">
                        <value>eloverblik</value>
                    </claim>
                </required-claims>
            </validate-jwt>
            <mock-response status-code="200" content-type="application/json" />
          </inbound>
        </policies>
      XML
    }
  ]
}
