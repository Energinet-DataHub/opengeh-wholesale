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

  tags = local.tags
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

  tags = local.tags
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
    azurerm_role_assignment.kv_self
  ]
}

resource "azurerm_key_vault_secret" "kvs_appi_connection_string" {
  name         = "AZURE-APPINSIGHTS-CONNECTIONSTRING"
  value        = azurerm_application_insights.this.connection_string
  key_vault_id = azurerm_key_vault.this.id

  lifecycle {
    ignore_changes = [
      tags,
    ]
  }

  depends_on = [
    azurerm_role_assignment.kv_self
  ]
}
