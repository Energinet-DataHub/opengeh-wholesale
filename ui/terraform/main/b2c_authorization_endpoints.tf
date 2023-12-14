locals {
  b2c_tenant_endpoint                = "https://${var.b2c_tenant_name}.b2clogin.com/${var.b2c_tenant_name}.onmicrosoft.com"
  b2c_authorization_sign_in_endpoint = "${local.b2c_tenant_endpoint}/${data.azurerm_key_vault_secret.authentication_sign_in_user_flow_id.value}/oauth2/v2.0/authorize"
  b2c_authorization_token_endpoint   = "${local.b2c_tenant_endpoint}/${data.azurerm_key_vault_secret.authentication_sign_in_user_flow_id.value}/oauth2/v2.0/token"
  b2c_authorization_invite_endpoint  = "${local.b2c_tenant_endpoint}/${data.azurerm_key_vault_secret.authentication_invitation_user_flow_id.value}/oauth2/v2.0/authorize"
  b2c_authorization_invite_uri       = "${local.b2c_authorization_invite_endpoint}?client_id=${azuread_application.frontend_app.application_id}&redirect_uri=https://${local.frontend_url}"
}
