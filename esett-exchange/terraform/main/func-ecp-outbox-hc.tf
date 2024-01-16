resource "null_resource" "ecp_outbox_biztalk_hybrid_connection" {

  triggers = {
    function_app_name = module.func_entrypoint_ecp_outbox.name
    resource_group_name  = azurerm_resource_group.this.name
    namespace_name = data.azurerm_key_vault_secret.relay_name.value
    hybrid_connection_name = data.azurerm_key_vault_secret.hc_biztalk_name.value
    subscription_id = var.subscription_id
  }

  provisioner "local-exec" {
    command = "az functionapp hybrid-connection add --hybrid-connection ${self.triggers.hybrid_connection_name} --namespace ${self.triggers.namespace_name} -n ${self.triggers.function_app_name} -g ${self.triggers.resource_group_name} --subscription ${self.triggers.subscription_id}"
  }

  provisioner "local-exec" {
    when = destroy
    command = "az functionapp hybrid-connection remove --hybrid-connection ${self.triggers.hybrid_connection_name} --namespace ${self.triggers.namespace_name} -n ${self.triggers.function_app_name} -g ${self.triggers.resource_group_name} --subscription ${self.triggers.subscription_id}"
  }
}
