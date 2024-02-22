module "apimao_ebix_dequeuemessage" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=v13"

  resource_group_name     = data.azurerm_key_vault_secret.apim_instance_resource_group_name.value
  api_management_name     = data.azurerm_key_vault_secret.apim_instance_name.value
  api_management_api_name = module.apima_b2b_ebix.name

  operation_id            = "dequeue-message"
  method                  = "POST"
  display_name            = "Dequeue message - ebIX"
  url_template            = "/?soapAction=dequeueMessage"

  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <set-variable name="messageId" value="@{
                  var body = context.Request.Body.As<XElement>();
                  XNamespace ns = "urn:www:datahub:dk:b2b:v01";
                  return body.Descendants(ns + "MessageId").First().Value;
                }" />
            <set-backend-service backend-id="${azurerm_api_management_backend.edi.name}" />
            <rewrite-uri template="@("/api/dequeue/" + context.Variables.GetValueOrDefault<string>("messageId"))" copy-unmatched-params="false" />
            <set-method>DELETE</set-method>
          </inbound>
          <backend>
            <base />
          </backend>
          <outbound>
            <base />
            <set-variable name="RequestId" value="@(context.RequestId)" />
            <choose>
              <when condition="@(context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)">
                <set-body template="liquid">
                  <soap-env:Envelope xmlns:soap-env="http://schemas.xmlsoap.org/soap/envelope/">
                    <soap-env:Body>
                      <DequeueMessageResponse xmlns:ns0="urn:www:datahub:dk:b2b:v01" />
                    </soap-env:Body>
                  </soap-env:Envelope>
                </set-body>
              </when>
              <when condition="@(context.Response.StatusCode == 400)">
                <set-status code="200" />
                <set-body template="liquid">
                  <soap-env:Envelope xmlns:soap-env="http://schemas.xmlsoap.org/soap/envelope/">
                    <soap-env:Body>
                      <soap-env:Fault>
                        <faultcode>soap-env:Client</faultcode>
                        <faultstring>B2B-201:{{context.Variables.RequestId}}</faultstring>
                        <faultactor />
                      </soap-env:Fault>
                    </soap-env:Body>
                  </soap-env:Envelope>
                </set-body>
              </when>
              <when condition="@(context.Response.StatusCode == 404)">
                  <set-status code="200" />
                  <set-body template="liquid">
                  <soap-env:Envelope xmlns:soap-env="http://schemas.xmlsoap.org/soap/envelope/">
                    <soap-env:Body>
                      <soap-env:Fault>
                        <faultcode>soap-env:Client</faultcode>
                        <faultstring>B2B-900:{{context.Variables.RequestId}}</faultstring>
                        <faultactor />
                      </soap-env:Fault>
                    </soap-env:Body>
                  </soap-env:Envelope>
                </set-body>
              </when>
            </choose>
          </outbound>
          <on-error>
            <base />
          </on-error>
        </policies>
      XML
    }
  ]
}
