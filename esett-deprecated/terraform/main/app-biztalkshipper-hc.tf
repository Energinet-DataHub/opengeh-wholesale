resource "azurerm_web_app_hybrid_connection" "biztalkshipper_biztalk" {
  web_app_id  = module.app_biztalkshipper.id
  relay_id    = azurerm_relay_hybrid_connection.biztalk.id
  hostname    = "datahub.preproduction.biztalk.energinet.local"
  port        = 443
}
