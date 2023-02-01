#
# Due to Azure limitation of integration subnet can be used by only one App Service plan, 
# we use the "old" integrations-subnet for VNet integrating the applications host in this environment
#
data "azurerm_key_vault_secret" "snet_vnet_integrations_id_performance_test" {
  name         = "snet-vnet-integrations-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}