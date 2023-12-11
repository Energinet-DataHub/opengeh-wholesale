locals {
  b2c_authorization_invite_uri = "${local.b2c_authorization_invite_endpoint}?client_id=${azuread_application.frontend_app.application_id}&redirect_uri=https://${local.frontend_url}"
}
