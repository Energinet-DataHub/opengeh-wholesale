resource "azurerm_relay_namespace" "this" {
  name                = "relay-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku_name            = "Standard"
}

module "kvs_relay_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "relay-name"
  value        = azurerm_relay_namespace.this.name
  key_vault_id = module.kv_shared.id
}

resource "azurerm_relay_hybrid_connection" "biztalk" {
  name                          = "hc-biztalk-${local.resources_suffix}"
  resource_group_name           = azurerm_resource_group.this.name
  relay_namespace_name          = azurerm_relay_namespace.this.name
  requires_client_authorization = false
  user_metadata                 ="[{\"key\":\"endpoint\",\"value\":\"${var.biztalk_hybrid_connection_hostname}\"}]"
}

module "kvs_hc_biztalk_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "hc-biztalk-id"
  value        = azurerm_relay_hybrid_connection.biztalk.id
  key_vault_id = module.kv_shared.id
}

module "kvs_hc_biztalk_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=13.33.2"

  name         = "hc-biztalk-name"
  value        = azurerm_relay_hybrid_connection.biztalk.name
  key_vault_id = module.kv_shared.id
}
