module "apima_ping" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api?ref=api-management-api_6.0.1"

  name                       = "ping"
  project_name               = var.domain_name_short
  api_management_name        = module.apim_shared.name
  resource_group_name        = azurerm_resource_group.this.name
  display_name               = "Ping API"
  authorization_server_name  = azurerm_api_management_authorization_server.oauth_server.name
  apim_logger_id             = azurerm_api_management_logger.apim_logger.id
  logger_sampling_percentage = 100.0
  logger_verbosity           = "error"
  path                       = "ping"
  # Always return 200 OK without sending any request to the backend
  policies = [
    {
      xml_content = <<XML
        <policies>
            <inbound>
                <base />
                <mock-response status-code="200" content-type="application/json" />
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

module "apimao_ping_api" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management-api-operation?ref=api-management-api-operation_6.0.1"

  operation_id            = "ping-api"
  api_management_api_name = module.apima_ping.name
  api_management_name     = module.apim_shared.name
  resource_group_name     = azurerm_resource_group.this.name
  display_name            = "Ping: API"
  method                  = "GET"
  url_template            = "/get"
}
