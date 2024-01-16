resource "azurerm_static_site" "ui" {
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
}

resource "azurerm_static_site_custom_domain" "this" {
  static_site_id  = azurerm_static_site.ui.id
  domain_name     = local.frontend_url
  validation_type = "cname-delegation"
}

module "kvs_stapp_ui_web_app_api_key" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=v13"

  name         = "stapp-ui-web-app-api-key"
  value        = azurerm_static_site.ui.api_key
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
