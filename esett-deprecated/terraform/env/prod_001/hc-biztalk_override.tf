resource "azurerm_relay_hybrid_connection" "biztalk" {
    user_metadata   = "[{\"key\":\"endpoint\",\"value\":\"datahub.biztalk.energinet.local:443\"}]"
}
