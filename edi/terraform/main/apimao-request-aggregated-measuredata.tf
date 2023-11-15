module "apimao_request_aggregated_measure_data" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=v12"

  operation_id            = "request-aggregated-measure-data"
  api_management_api_name = module.apima_b2b.name
  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  display_name            = "EDI: Request aggregated measure data"
  method                  = "POST"
  url_template            = "/v1.0/cim/requestaggregatedmeasuredata"
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
                        <value>metereddataresponsible</value>
                        <value>energysupplier</value>
                        <value>balanceresponsibleparty</value>
                    </claim>
                </required-claims>
            </validate-jwt>
            <set-backend-service backend-id="${azurerm_api_management_backend.edi.name}" />
            <rewrite-uri template="/api/RequestAggregatedMeasureMessageReceiver" />
          </inbound>
        </policies>
      XML
    }
  ]
}
