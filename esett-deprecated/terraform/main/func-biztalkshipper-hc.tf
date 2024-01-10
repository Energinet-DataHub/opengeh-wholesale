resource "azurerm_function_app_hybrid_connection" "biztalkshipper_biztalk" {
  function_app_id = module.func_biztalkshipper.id
  relay_id        = data.azurerm_key_vault_secret.hc_biztalk_id.value
  hostname        = var.biztalk_hybrid_connection_hostname
  port            = 443

  lifecycle {
    ignore_changes = [
      relay_id
    ]
  }

  # This is a workaround for a bug in the azurerm provider, for more info see:
  # - https://stigvoss.dk/2023/11/16/creating-hybrid-connection-with-terraform/
  provisioner "local-exec" {
    command = "az functionapp hybrid-connection add --hybrid-connection ${data.azurerm_key_vault_secret.hc_biztalk_name.value} --namespace ${data.azurerm_key_vault_secret.relay_name.value} -n ${module.func_biztalkreceiver.name} -g ${data.azurerm_resource_group.shared.name} --subscription ${var.subscription_id}"
  }
}
