resource "azurerm_function_app_hybrid_connection" "biztalkshipper_biztalk" {
  function_app_id = module.func_biztalkshipper.id
  relay_id        = azurerm_relay_hybrid_connection.biztalk.id
  hostname        = "datahub.preproduction.biztalk.energinet.local"
  port            = 443

  lifecycle {
    ignore_changes = [
      relay_id
    ]
  }

  # This is a workaround for a bug in the azurerm provider, for more info see:
  # - https://stigvoss.dk/2023/11/16/creating-hybrid-connection-with-terraform/
  provisioner "local-exec" {
    command = "az functionapp hybrid-connection add --hybrid-connection ${azurerm_relay_hybrid_connection.biztalk.name} --namespace ${azurerm_relay_namespace.this.name} -n ${module.func_biztalkreceiver.name} -g ${azurerm_resource_group.this.name} --subscription ${var.subscription_id}"
  }
}
