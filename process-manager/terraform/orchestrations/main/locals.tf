locals {
  # Logging
  LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT                           = "Information" # Everything else
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE            = "Information" # From geh-core
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_PROCESS_MANAGER = "Information" # From opengeh-process-manager

  OrchestrationsTaskHubName = data.azurerm_key_vault_secret.st_taskhub_hub_name.value

  # Outlaw stuff
  tags = {
    "BusinessServiceName"   = "Datahub",
    "BusinessServiceNumber" = "BSN10136"
  }

  # IP Whitelist
  ip_restrictions_as_string = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])
}
