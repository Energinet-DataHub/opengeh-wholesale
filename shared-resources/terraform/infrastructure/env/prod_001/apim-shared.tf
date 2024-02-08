module "apim_shared" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/api-management?ref=13.37.0"

  name                 = "shared"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  publisher_name       = var.project_name
  publisher_email      = var.apim_publisher_email
  sku_name             = "Developer_1"
  virtual_network_type = "External"
  subnet_id            = data.azurerm_subnet.snet_apim.id

  policies = [
    {
      xml_content = <<XML
        <policies>
          <inbound>
            <choose>
              <when condition="@(${var.apim_maintenance_mode})">
                <return-response>
                  <set-status code="503" reason="Service Unavailable"/>
                  <set-body>DataHub is in maintenance mode.</set-body>
                </return-response>
              </when>
            </choose>
          </inbound>
          <backend>
            <forward-request />
          </backend>
          <outbound />
          <on-error />
        </policies>
      XML
    }
  ]

  certificates = [
    {
      encoded_certificate = filebase64("./certificates/DH3-test-mosaik-1-OCES-root-CA-binary.cer")
      store_name          = "Root"
    }
  ]
}

resource "azurerm_api_management_authorization_server" "oauth_server" {
  name                         = "oauthserver"
  api_management_name          = module.apim_shared.name
  resource_group_name          = azurerm_resource_group.this.name
  display_name                 = "OAuth client credentials server"
  client_registration_endpoint = "http://localhost/"
  grant_types = [
    "clientCredentials",
  ]
  authorization_endpoint = "https://login.microsoftonline.com/${var.apim_b2c_tenant_id}/oauth2/v2.0/authorize"
  authorization_methods = [
    "GET",
  ]
  token_endpoint = "https://login.microsoftonline.com/${var.apim_b2c_tenant_id}/oauth2/v2.0/token"
  client_authentication_method = [
    "Body",
  ]
  bearer_token_sending_methods = [
    "authorizationHeader",
  ]
  default_scope = "api://${var.backend_b2b_app_id}/.default"
  client_id     = var.backend_b2b_app_id
}

resource "azurerm_key_vault_access_policy" "certificate_permissions" {
  key_vault_id = module.kv_shared.id

  object_id = module.apim_shared.identity.0.principal_id
  tenant_id = module.apim_shared.identity.0.tenant_id

  certificate_permissions = [
    "Get",
    "List",
    "Import"
  ]

  secret_permissions = [
    "Get"
  ]
}

resource "azurerm_api_management_logger" "apim_logger" {
  name                = "apim-logger"
  api_management_name = module.apim_shared.name
  resource_group_name = azurerm_resource_group.this.name
  resource_id         = module.appi_shared.id

  application_insights {
    instrumentation_key = module.appi_shared.instrumentation_key
  }
}

# The if-else constructor is added to circumvent a bug causing the principal_id to be null during the TF plan.
# The if-else constructor triggers a bug (named "Error: Provider produced inconsistent final plan") in the TF provider caused at TF apply time.
# Once re-run, the Key vault secret is created as expected.
module "kvs_apim_principal_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "apim-principal-id"
  value        = module.apim_shared.identity[0].principal_id == null ? "" : module.apim_shared.identity[0].principal_id
  key_vault_id = module.kv_shared.id
}

module "kvs_apim_gateway_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "apim-gateway-url"
  value        = module.apim_shared.gateway_url
  key_vault_id = module.kv_shared.id
}

module "kvs_apim_logger_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "apim-logger-id"
  value        = azurerm_api_management_logger.apim_logger.id
  key_vault_id = module.kv_shared.id
}

module "kvs_apim_instance_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "apim-instance-id"
  value        = module.apim_shared.id
  key_vault_id = module.kv_shared.id
}

module "kvs_apim_instance_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "apim-instance-name"
  value        = module.apim_shared.name
  key_vault_id = module.kv_shared.id
}

module "kvs_apim_instance_resource_group_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "apim-instance-resource-group-name"
  value        = azurerm_resource_group.this.name
  key_vault_id = module.kv_shared.id
}

module "kvs_b2c_tenant_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "b2c-tenant-id"
  value        = var.apim_b2c_tenant_id
  key_vault_id = module.kv_shared.id
}

module "kvs_apim_oauth_server_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "apim-oauth-server-name"
  value        = azurerm_api_management_authorization_server.oauth_server.name
  key_vault_id = module.kv_shared.id
}
