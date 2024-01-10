
resource "azurerm_function_app_hybrid_connection" "biztalkshipper_biztalk" {
  function_app_id = module.func_biztalkshipper.id
  relay_id        = data.azurerm_key_vault_secret.hc_biztalk_id.value
  hostname        = var.biztalk_hybrid_connection_hostname
  port            = 443
}
