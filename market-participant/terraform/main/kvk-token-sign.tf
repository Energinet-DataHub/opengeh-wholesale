resource "azurerm_key_vault_key" "token_sign" {
  name         = "token-sign"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
  key_type     = "RSA"
  key_size     = 2048

  key_opts = [
    "sign",
  ]
}
