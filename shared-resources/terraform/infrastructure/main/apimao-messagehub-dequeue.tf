module "apimao_messagehub_dequeue" {
  source                  = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=v10"

  operation_id            = "dequeue"
  api_management_api_name = module.apima_b2b.name
  resource_group_name     = azurerm_resource_group.this.name
  api_management_name     = module.apim_shared.name
  display_name            = "Message Hub: Dequeue"
  method                  = "DELETE"
  url_template            = "/v1.0/cim/dequeue/{id}"
  template_parameters     = [
    {
      name      = "id"
      required  = true
      type      = "string"
    }
  ]
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
                        <value>meterdataresponsible</value>
                        <value>balanceresponsibleparty</value>
                        <value>gridoperator</value>
                        <value>electricalsupplier</value>
                        <value>transmissionsystemoperator</value>
                        <value>imbalancesettlementresponsible</value>
                        <value>meteringpointadministrator</value>
                        <value>metereddataadministrator</value>
                        <value>systemoperator</value>
                        <value>danishenergyagency</value>
                    </claim>
                </required-claims>
            </validate-jwt>
            <set-backend-service backend-id="${azurerm_api_management_backend.market_roles.name}" />
            <rewrite-uri template="/api/dequeue/{id}" />
          </inbound>
        </policies>
      XML
    }
  ]
}
