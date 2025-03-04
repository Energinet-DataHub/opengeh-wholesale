locals {
  # Product goals to be in App Configuration is defined by Kristian Brønner (XKVBR)
  # Description for each product goal can be found here: https://energinet.atlassian.net/wiki/spaces/DataHub/pages/1410236417/Release+Toggles+i+Azure
  # Key = ID, value = Description consisting of title - description
  product_goals = {
    "PM25-CIM"                             = "PM25: Modtagelse af måledata (CIM JSON & CIM XML) - Denne toggle dækker over det samlede scope for funktionaliteten, der omhandler 'Modtagelse af måledata'. Toggles skal sikre, at vi kan teste funktionaliteten i PreProd, inden den bliver aktiveret i produktion.",
    "PM25-EBIX"                            = "PM25: Indsendelse af måledata (eBix) - Denne toggle skal isolere formatet eBix. På den måde kan vi udgive 'Indsendelse af måledata' i andre formater, inden vi er færdige med at udvikle på eBix-formatet.",
    "PM25-MESSAGES"                        = "PM25: Hentning af måledatabeskeder - Denne toggle skal styre, hvor og hvornår vi aktiverer muligheden for, at brugerne kan hente deres måledatabeskeder i produktion.",
    "PM25-SYSTEMCORRECTION-PROCESSMANAGER" = "PM25: Send nettab- og systemkorrektion via process manager i stedet for til DH2 - Da nettab- og systemkorrektion ikke længere skal sendes til DH2, skal data i stedet sendes via DH2 bridge ind gennem process manager og derfra videre til migration.",
    "PM34-MISSINGDATALOG"                  = "PM34: Send hullerlog til netvirksomheder - Denne toggle skal gøre, at vi kan styre, hvornår funktionaliteten kan aktiveres i produktion, samt hjælpe os med at teste funktionaliteten i PreProd, før den bliver aktiv i produktion.",
    "PM27-ELECTRICALHEATING"               = "PM27: Udsendelse af beregnet ElVarme - Denne toggle skal sikre, at vi kan køre vores beregner i produktion, men samtidig have kontrol over, hvornår selve udsendelsesfunktionen bliver aktiveret.",
    "PM11-CAPACITYSETTLEMENT"              = "PM11: Udsendelse af beregnet Effektbetaling - Denne toggle skal sikre, at vi kan køre vores beregner i produktion, men samtidig have kontrol over, hvornår selve udsendelsesfunktionen bliver aktiveret.",
    "PM26-NETSETTLEMENT"                   = "PM26: Udsendelse af beregnet Nettoafregning - Denne toggle skal sikre, at vi kan køre vores beregner i produktion, men samtidig have kontrol over, hvornår selve udsendelsesfunktionen bliver aktiveret.",
    "PM31-REPORTS"                         = "PM31: Rapporter til aktører - Denne toggle skal sikre, at vi kan styre, hvornår vi ønsker at frigive funktionaliteten i frontenden.",
    "PM88-INTERNALREPORTS"                 = "PM88: Interne rapporter til support af Datahub-drift - Denne toggle skal sikre, at vi kan styre, hvornår vi ønsker at frigive funktionaliteten i frontenden.",
    "PM28-CIM"                             = "PM28: Anmodning om måledata Årssum og variable opløsninger (perioder) B2B (CIM JSON & CIM XML) - Denne toggle skal isolere formaterne CIM JSON & CIM XML. På den måde kan vi udgive 'Anmodning om måledata, Årssum og variable opløsninger' i andre formater, inden vi er færdige med at udvikle på eBix-formatet.",
    "PM28-EBIX"                            = "PM28: Anmodning om måledata Årssum og variable opløsninger (perioder) B2B (eBix) - Denne toggle skal isolere formatet eBix. På den måde kan vi udgive 'Anmodning om måledata, Årssum og variable opløsninger' i andre formater, inden vi er færdige med at udvikle på eBix-formatet.",
    "PM96-SHAREMEASUREDATA"                = "PM96: Fremsend måledata via UI - Denne toggle skal sikre, at vi kan teste funktionaliteten i PreProd, samt styre hvornår vi ønsker at frigive den i produktion.",
    "MEASUREDATA-MIGRATION"                = "Måledata fra Migration - Denne toggle skal sikre, at vi kan lave en 'cutover' fra, at data kommer fra subsystemet 'Migration' til 'Measurement'.",
    "MEASUREDATA-MEASUREMENTS"             = "Måledata fra Measurements - Denne toggle skal sikre, at vi kan lave en 'cutover' fra, at data kommer fra subsystemet 'Migration' til 'Measurement'.",
    "PM41-METERINGPOINTMASTERDATA"         = "Adgang til målepunktsstamdata - Denne toggle dækker over det samlede scope for PM41 Målepunktsstamdata. Togglen skal sikre, at vi tester i PreProd, inden den bliver aktiveret i produktion."
  }
}

resource "azurerm_app_configuration" "release" {
  name                  = "appcs-${local.resources_suffix}"
  resource_group_name   = azurerm_resource_group.this.name
  location              = azurerm_resource_group.this.location
  sku                   = var.app_configuration_sku
  local_auth_enabled    = false
  public_network_access = "Enabled" # Allows management to access the App Configuration from the Azure Portal

  # When provider is updated to 4.19.0 or later enable this to follow best practices - it won't affect the functionality
  # data_plane_proxy_authentication_mode = "Pass-through"

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

resource "azurerm_monitor_diagnostic_setting" "app_configuration_audit" {
  count = var.enable_audit_logs ? 1 : 0

  name               = "audit"
  target_resource_id = azurerm_app_configuration.release.id
  storage_account_id = module.st_audit_logs.id

  enabled_log {
    category_group = "audit"
  }

  lifecycle {
    # For some reason Azure shows changes to null each time...
    ignore_changes = [
      metric
    ]
  }
}
