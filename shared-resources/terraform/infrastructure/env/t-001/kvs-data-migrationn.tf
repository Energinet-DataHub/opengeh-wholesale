module "kvs_sas_token_datamig_charges" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "datamig-charges"
  value         = var.sas_token_datamig_charges
  key_vault_id  = module.kv_shared.id
}

module "kvs_sas_token_datamig_market_roles" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "datamig-market-roles"
  value         = var.sas_token_datamig_market_roles
  key_vault_id  = module.kv_shared.id
}

module "kvs_sas_token_datamig_metering_point" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "datamig-metering-point"
  value         = var.sas_token_datamig_metering_point
  key_vault_id  = module.kv_shared.id
}

module "kvs_sas_token_datamig_time_series" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "datamig-time-series"
  value         = var.sas_token_datamig_time_series
  key_vault_id  = module.kv_shared.id
}

module "kvs_sas_token_datamig_wholesale" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v10"

  name          = "datamig-wholesale"
  value         = var.sas_token_datamig_wholesale
  key_vault_id  = module.kv_shared.id
}