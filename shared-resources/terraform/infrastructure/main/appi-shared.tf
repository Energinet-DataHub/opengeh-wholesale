module "appi_shared" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/application-insights?ref=application-insights_5.0.0"

  project_name               = var.domain_name_short
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  log_analytics_workspace_id = module.log_workspace_shared.id
  storage_account_sourcemaps = module.st_source_maps.name
}

resource "azurerm_application_insights_standard_web_test" "datahub3dk" {
  name                    = "datahub3-ui"
  resource_group_name     = azurerm_resource_group.this.name
  location                = azurerm_resource_group.this.location
  application_insights_id = module.appi_shared.id
  enabled                 = true
  geo_locations           = ["emea-nl-ams-azr", "emea-gb-db3-azr"] # WE, NE, see https://learn.microsoft.com/da-dk/previous-versions/azure/azure-monitor/app/monitor-web-app-availability#location-population-tags

  request {
    url = var.environment_instance == "001" ? (var.environment == "prod" ? "https://datahub3.dk" : "https://${var.environment}.datahub3.dk") : "https://${var.environment}${var.environment_instance}.datahub3.dk"
  }

  validation_rules {
    ssl_check_enabled           = true
    ssl_cert_remaining_lifetime = 7
  }
  tags = local.tags
}

resource "azurerm_application_insights_standard_web_test" "b2b" {
  name                    = "datahub3-b2b"
  resource_group_name     = azurerm_resource_group.this.name
  location                = azurerm_resource_group.this.location
  application_insights_id = module.appi_shared.id
  enabled                 = true
  geo_locations           = ["emea-nl-ams-azr", "emea-gb-db3-azr"] # West Europe, see https://learn.microsoft.com/da-dk/previous-versions/azure/azure-monitor/app/monitor-web-app-availability#location-population-tags

  request {
    url = var.environment_instance == "001" ? (var.environment == "prod" ? "https://b2b.datahub3.dk/ping/get" : "https://${var.environment}.b2b.datahub3.dk/ping/get") : "https://${var.environment}${var.environment_instance}.b2b.datahub3.dk/ping/get"
  }

  validation_rules {
    ssl_check_enabled           = true
    ssl_cert_remaining_lifetime = 7
  }

  tags = local.tags
}

module "kvs_appi_shared_connection_string" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_5.0.0"

  name         = "appi-shared-connection-string"
  value        = module.appi_shared.connection_string
  key_vault_id = module.kv_shared.id
}

module "kvs_appi_shared_instrumentation_key" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_5.0.0"

  name         = "appi-shared-instrumentation-key"
  value        = module.appi_shared.instrumentation_key
  key_vault_id = module.kv_shared.id
}

module "kvs_appi_shared_name" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_5.0.0"

  name         = "appi-shared-name"
  value        = module.appi_shared.name
  key_vault_id = module.kv_shared.id
}

module "kvs_appi_shared_id" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=key-vault-secret_5.0.0"

  name         = "appi-shared-id"
  value        = module.appi_shared.id
  key_vault_id = module.kv_shared.id
}
