removed {
  from = azurerm_static_site.ui
  lifecycle {
    destroy = false
  }
}

import {
  id = "/subscriptions/${data.azurerm_client_config.current.subscription_id}/resourceGroups/${azurerm_resource_group.this.name}/providers/Microsoft.Web/staticSites/stapp-ui-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  to = azurerm_static_web_app.ui
}


resource "azurerm_static_web_app" "ui" {
  name                = "stapp-ui-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku_size            = "Standard"
  sku_tier            = "Standard"

  lifecycle {
    ignore_changes = [
      # Ignore changes to tags, e.g. because a management agent
      # updates these based on some ruleset managed elsewhere.
      tags,
    ]
  }

  tags = local.tags
}

module "kvs_stapp_ui_web_app_api_key" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "stapp-ui-web-app-api-key"
  value        = azurerm_static_web_app.ui.api_key
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
