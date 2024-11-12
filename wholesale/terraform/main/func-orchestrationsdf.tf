module "kvs_func_orchestrationsdf_base_url" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "func-wholesale-orchestrationsdf-base-url"
  value        = "https://func-orchestrationsdf-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${local.region_code}-${lower(var.environment_instance)}"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

