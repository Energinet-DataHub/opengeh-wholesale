module "kvs_backend_b2b_app_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "backend-b2b-app-id"
  value        = var.backend_b2b_app_id
  key_vault_id = module.kv_shared.id
}

module "kvs_backend_b2b_app_obj_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "backend-b2b-app-obj-id"
  value        = var.backend_b2b_app_obj_id
  key_vault_id = module.kv_shared.id
}

module "kvs_backend_b2b_app_sp_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "backend-b2b-app-sp-id"
  value        = var.backend_b2b_app_sp_id
  key_vault_id = module.kv_shared.id
}

module "kvs_backend_timeseriesapi_app_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "backend-timeseriesapi-app-id"
  value        = var.backend_timeseriesapi_app_id
  key_vault_id = module.kv_shared.id
}

module "kvs_eloverblik_timeseriesapi_client_app_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "eloverblik-timeseriesapi-client-app-id"
  value        = var.eloverblik_timeseriesapi_client_app_id
  key_vault_id = module.kv_shared.id
}

module "kvs_backend_bff_app_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "backend-bff-app-id"
  value        = var.backend_bff_app_id
  key_vault_id = module.kv_shared.id
}

module "kvs_backend_bff_app_sp_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "backend-bff-app-sp-id"
  value        = var.backend_bff_app_sp_id
  key_vault_id = module.kv_shared.id
}

module "kvs_backend_bff_app_scope_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "backend-bff-app-scope-id"
  value        = var.backend_bff_app_scope_id
  key_vault_id = module.kv_shared.id
}

module "kvs_backend_bff_app_scope" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "backend-bff-app-scope"
  value        = var.backend_bff_app_scope
  key_vault_id = module.kv_shared.id
}

module "kvs_mitid_frontend_open_id_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "mitid-frontend-open-id-url"
  value        = var.mitid_frontend_open_id_url
  key_vault_id = module.kv_shared.id
}

module "kvs_frontend_open_id_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "frontend-open-id-url"
  value        = var.frontend_open_id_url
  key_vault_id = module.kv_shared.id
}

module "kvs_authentication_sign_in_user_flow_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "authentication-sign-in-user-flow-id"
  value        = var.authentication_sign_in_user_flow_id
  key_vault_id = module.kv_shared.id
}

module "kvs_authentication_invitation_user_flow_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "authentication-invitation-user-flow-id"
  value        = var.authentication_invitation_user_flow_id
  key_vault_id = module.kv_shared.id
}

module "kvs_authentication_mitid_signup_signin_user_flow_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_4.0.1"

  name         = "authentication-mitid-signup-signin-user-flow-id"
  value        = var.authentication_mitid_signup_signin_user_flow_id
  key_vault_id = module.kv_shared.id
}
