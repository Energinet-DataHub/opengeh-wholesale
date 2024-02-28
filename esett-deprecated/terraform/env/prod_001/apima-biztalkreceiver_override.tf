module "apim_biztalkreceiver" {
  count = 0
  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <set-backend-service backend-id="biztalkreceiver" />
            <ip-filter action="allow">
              <address>194.239.2.103</address>
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

resource "azurerm_api_management_backend" "biztalkreceiver" {
  count = 0
}

module "apimao_receiverfunction" {
  count = 0
}
