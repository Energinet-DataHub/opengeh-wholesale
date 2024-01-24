module "apim_biztalk_inbox" {
  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <set-backend-service backend-id="${resource.azurerm_api_management_backend.biztalk_inbox_backend[0].name}" />
            <ip-filter action="allow">
              <address>194.239.2.105</address>
            </ip-filter>
          </inbound>
          <backend>
            <base />
          </backend>
          <outbound>
            <base />
          </outbound>
          <on-error>
            <base />
          </on-error>
        </policies>
      XML
    }
  ]
}
