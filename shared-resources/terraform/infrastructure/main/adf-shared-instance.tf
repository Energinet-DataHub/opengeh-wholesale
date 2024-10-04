resource "azurerm_data_factory" "this" {
  name                            = "adf-${local.resources_suffix}"
  location                        = azurerm_resource_group.this.location
  resource_group_name             = azurerm_resource_group.this.name
  managed_virtual_network_enabled = true
  public_network_enabled          = false
  identity {
    type = "SystemAssigned"
  }

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  tags = local.tags
}

resource "azurerm_monitor_metric_alert" "adf_failed_runs" {
  count = var.alert_email_address != null ? 1 : 0

  name                = "adf-failed-run-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  scopes              = [azurerm_data_factory.this.id]
  description         = "Action will be triggered if a pipeline run fails."
  severity            = 0     # Critical
  auto_mitigate       = false # We want to handle it manually and check up on the issue

  criteria {
    metric_namespace = "Microsoft.DataFactory/factories"
    metric_name      = "PipelineFailedRuns"
    aggregation      = "Total"
    operator         = "GreaterThan"
    threshold        = 0
  }

  action {
    action_group_id = module.monitor_action_group_shres[0].id
  }

  tags = local.tags
}

module "kvs_azure_data_factory_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_5.0.0"

  name         = "adf-id"
  value        = azurerm_data_factory.this.id
  key_vault_id = module.kv_shared.id
}

module "kvs_azure_data_factory_principal_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_5.0.0"

  name         = "adf-principal-id"
  value        = azurerm_data_factory.this.identity[0].principal_id
  key_vault_id = module.kv_shared.id
}
