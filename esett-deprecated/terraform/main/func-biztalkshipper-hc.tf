resource "azurerm_function_app_hybrid_connection" "biztalkshipper_biztalk" {
  function_app_id = module.func_biztalkshipper.id
  relay_id        = azurerm_relay_hybrid_connection.biztalk.id
  hostname        = "datahub.preproduction.biztalk.energinet.local"
  port            = 443
}
