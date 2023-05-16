resource "time_offset" "token_sign_initial_expiration_date" {
  offset_months = 11
}

resource "azurerm_key_vault_key" "token_sign" {
  name            = "token-sign"
  key_vault_id    = data.azurerm_key_vault.kv_shared_resources.id
  key_type        = "RSA"
  key_size        = 2048
  expiration_date = time_offset.token_sign_initial_expiration_date.rfc3339

  key_opts = [
    "sign",
  ]

  rotation_policy {
    automatic {
      time_before_expiry  = "P30D"
    }

    expire_after         = "P11M"
    notify_before_expiry = "P30D"
  }
}
