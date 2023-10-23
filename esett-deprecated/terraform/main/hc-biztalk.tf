resource "azurerm_relay_hybrid_connection" "biztalk" {
  name                                      = "hc-bizztalk-${local.name_suffix}"
  resource_group_name                       = azurerm_resource_group.this.name
  relay_namespace_name                      = azurerm_relay_namespace.this.name
  requires_client_authorization             = false
  user_metadata                             ="[{\"key\":\"endpoint\",\"value\":\"datahub.preproduction.biztalk.energinet.local:443\"}]"
}
