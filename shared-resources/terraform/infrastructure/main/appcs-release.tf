resource "azurerm_app_configuration" "release" {
  name                = "appcs-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku                 = var.app_configuration_sku
  local_auth_enabled  = false

  identity {
    type = "SystemAssigned"
  }

  tags = local.tags
}

# Allow SPN to create and manage App Configuration, as the operations are on data plane
resource "azurerm_role_assignment" "app_configuration" {
  scope                = azurerm_app_configuration.release.id
  role_definition_name = "App Configuration Data Owner"
  principal_id         = data.azurerm_client_config.current.object_id
}

resource "azurerm_role_assignment" "release_toggle_managers" {
  scope                = azurerm_app_configuration.release.id
  role_definition_name = "App Configuration Data Owner"
  principal_id         = data.azuread_group.release_toggle_managers.object_id
}

resource "azurerm_role_assignment" "release_toggle_managers_read" {
  scope                = azurerm_app_configuration.release.id
  role_definition_name = "App Configuration Reader"
  principal_id         = data.azuread_group.release_toggle_managers.object_id
}

locals {
  # Product goals to be in App Configuration is defined by Kristian Brønner (XKVBR)
  # Description for each product goal can be found here: https://energinet.atlassian.net/wiki/spaces/DataHub/pages/1235615745/DataHub+-+2025+Leverancer
  product_goals = {
    "PM22" = "Afvikling af anmodninger og beregninger i ProcessManager",
    "PM47" = "Anmodninger om måledata for aggregerede værdier fra DataHub2",
    "PM36" = "Sammenlægning af netområder",
    "PM41" = "Målepunktstamdata og leverandør forhold til måledata",
    "PM25" = "Fremsendelse af måledata",
    "PM27" = "Beregninger af elvarmemålepunktet",
    "PM11" = "Effektbetaling",
    "PM26" = "Beregning af estimeret årsnettoforbrug og slutopgørelse (Solcelle afregning)",
    "PM28" = "Anmodning om måledata -Årssum og variable opløsninger (perioder)",
    "PM29" = "Tillægsmodtagere af måledata",
    "PM31" = "Rapporter til aktører (kontrolrapporter, Måledatarapporter)",
    "PM33" = "Måledata til Energy T&T",
    "PM34" = "Send rykkere til netvirksomheden",
    "PM32" = "Måledata til SAP"
  }
}

resource "azurerm_app_configuration_feature" "features" {
  for_each = local.product_goals

  configuration_store_id = azurerm_app_configuration.release.id
  description            = each.value
  name                   = each.key
  enabled                = false
  locked                 = false

  # This allows management to manually toggle features on/off
  lifecycle {
    ignore_changes = [
      enabled,
      locked
    ]
  }

  depends_on = [azurerm_role_assignment.app_configuration]
}

module "kvs_app_configuration_endpoint" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "app-configuration-shared-endpoint"
  value        = azurerm_app_configuration.release.endpoint
  key_vault_id = module.kv_shared.id
}

module "kvs_app_configuration_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_6.0.0"

  name         = "app-configuration-shared-id"
  value        = azurerm_app_configuration.release.id
  key_vault_id = module.kv_shared.id
}
