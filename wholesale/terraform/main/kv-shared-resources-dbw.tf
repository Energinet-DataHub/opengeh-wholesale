data "azurerm_key_vault_secret" "main_virtual_network_id" {
  name         = "vnet-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "main_virtual_network_name" {
  name         = "vnet-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "main_virtual_network_resource_group_name" {
  name         = "vnet-resource-group-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "dns_resolver_forwarding_ruleset_id" {
  name         = "dns-resolver-forwarding-ruleset-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
