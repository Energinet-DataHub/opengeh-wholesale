module "kvs_backend_b2b_app_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "backend-b2b-app-id"
  value        = var.backend_b2b_app_id
  key_vault_id = module.kv_shared.id
}

module "kvs_backend_b2b_app_obj_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "backend-b2b-app-obj-id"
  value        = var.backend_b2b_app_obj_id
  key_vault_id = module.kv_shared.id
}

module "kvs_backend_b2b_app_sp_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "backend-b2b-app-sp-id"
  value        = var.backend_b2b_app_sp_id
  key_vault_id = module.kv_shared.id
}

module "kvs_backend_timeseriesapi_app_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "backend-timeseriesapi-app-id"
  value        = var.backend_timeseriesapi_app_id
  key_vault_id = module.kv_shared.id
}

module "kvs_eloverblik_timeseriesapi_client_app_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "eloverblik-timeseriesapi-client-app-id"
  value        = var.eloverblik_timeseriesapi_client_app_id
  key_vault_id = module.kv_shared.id
}

module "kvs_backend_bff_app_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "backend-bff-app-id"
  value        = var.backend_bff_app_id
  key_vault_id = module.kv_shared.id
}

module "kvs_backend_bff_app_sp_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "backend-bff-app-sp-id"
  value        = var.backend_bff_app_sp_id
  key_vault_id = module.kv_shared.id
}

module "kvs_backend_bff_app_scope_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "backend-bff-app-scope-id"
  value        = var.backend_bff_app_scope_id
  key_vault_id = module.kv_shared.id
}

module "kvs_backend_bff_app_scope" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "backend-bff-app-scope"
  value        = var.backend_bff_app_scope
  key_vault_id = module.kv_shared.id
}

module "kvs_frontend_open_id_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "frontend-open-id-url"
  value        = var.frontend_open_id_url
  key_vault_id = module.kv_shared.id
}

module "kvs_authentication_sign_in_user_flow_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "authentication-sign-in-user-flow-id"
  value        = var.authentication_sign_in_user_flow_id
  key_vault_id = module.kv_shared.id
}

module "kvs_authentication_invitation_user_flow_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "authentication-invitation-user-flow-id"
  value        = var.authentication_invitation_user_flow_id
  key_vault_id = module.kv_shared.id
}

module "kvs_authentication_mitid_invitation_user_flow_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.32.0"

  name         = "authentication-mitid-invitation-user-flow-id"
  value        = var.authentication_mitid_invitation_user_flow_id
  key_vault_id = module.kv_shared.id
}
