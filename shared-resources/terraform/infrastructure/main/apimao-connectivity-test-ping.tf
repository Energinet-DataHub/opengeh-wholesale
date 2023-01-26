module "apimao_ping_for_connectivity_test" {
  source                  = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=v10"

  operation_id            = "connectivity-test-ping"
  api_management_api_name = module.apima_b2b.name
  resource_group_name     = azurerm_resource_group.this.name
  api_management_name     = module.apim_shared.name
  display_name            = "Connectivity test: ping 204"
  method                  = "GET"
  url_template            = "/ping"
  policies                = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <validate-jwt header-name="Authorization" failed-validation-httpcode="403" failed-validation-error-message="Unauthorized to access this endpoint.">
                <openid-config url="https://login.microsoftonline.com/${var.apim_b2c_tenant_id}/v2.0/.well-known/openid-configuration" />
                <required-claims>
                    <claim name="roles" match="any">
                        <value>gridoperator</value>
                        <value>electricalsupplier</value>
                        <value>transmissionsystemoperator</value>
                        <value>balanceresponsibleparty</value>
                        <value>meterdataresponsible</value>
                    </claim>
                </required-claims>
            </validate-jwt>
            <mock-response status-code="204" content-type="application/json" />
          </inbound>
        </policies>
      XML
    }
  ]
}
