module "apimao_request_wholesale_settlement" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=v13"

  operation_id            = "request-request-wholesale-settlement"
  api_management_api_name = module.apima_b2b.name
  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  display_name            = "EDI: Request wholesale settlement"
  method                  = "POST"
  url_template            = "/v1.0/cim/requestwholesalesettlement"
  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <validate-jwt header-name="Authorization" failed-validation-httpcode="403" failed-validation-error-message="Unauthorized to access this endpoint.">
                <openid-config url="https://login.microsoftonline.com/${data.azurerm_key_vault_secret.apim_b2c_tenant_id.value}/v2.0/.well-known/openid-configuration" />
                <required-claims>
                    <claim name="roles" match="any">
                        <value>energysupplier</value>
                        <value>delegated</value>
                        <value>systemoperator</value>
                        <value>gridaccessprovider</value>
                    </claim>
                </required-claims>
            </validate-jwt>
            <set-backend-service backend-id="${azurerm_api_management_backend.edi.name}" />
            <rewrite-uri template="/api/incomingMessages/RequestWholesaleSettlement" />
          </inbound>
        </policies>
      XML
    }
  ]
}
