resource "azurerm_log_analytics_workspace" "this" {
  name                = "log-${local.resource_suffix_with_dash}"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  sku                 = "PerGB2018"
  retention_in_days   = 30

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

resource "azurerm_application_insights" "this" {
  name                = "appi-${local.resource_suffix_with_dash}"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }
}

resource "azurerm_key_vault_secret" "kvs_log_workspace_id" {
  name         = "AZURE-LOGANALYTICS-WORKSPACE-ID"
  value        = azurerm_log_analytics_workspace.this.workspace_id
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}

resource "azurerm_key_vault_secret" "kvs_appi_instrumentation_key" {
  name         = "AZURE-APPINSIGHTS-INSTRUMENTATIONKEY"
  value        = azurerm_application_insights.this.instrumentation_key
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_key_vault_access_policy.kv_selfpermission
  ]
}
