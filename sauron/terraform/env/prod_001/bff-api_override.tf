# // sauron.test002.datahub3.dk can be removed after Sauron is using environment variables and not hardcoded values
module "apima_bff_api" {
  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <base />
            <set-header name="RequestTime" exists-action="override">
                <value>@(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))</value>
            </set-header>
            <cors allow-credentials="true">
                <allowed-origins>
                    <origin>https://${local.frontend_url}</origin>
                    <origin>https://sauron.test002.datahub3.dk</origin>
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
          </outbound>
          <on-error>
              <base />
          </on-error>
        </policies>
      XML
    }
  ]
}
