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
