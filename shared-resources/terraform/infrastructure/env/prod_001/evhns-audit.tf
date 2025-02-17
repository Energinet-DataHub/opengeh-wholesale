module "evhns_audit" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/eventhub-namespace?ref=eventhub-namespace_8.1.0"

  project_name               = "audit${var.domain_name_short}"
  environment_short          = var.environment_short
  environment_instance       = var.environment_instance
  resource_group_name        = azurerm_resource_group.this.name
  location                   = azurerm_resource_group.this.location
  private_endpoint_subnet_id = azurerm_subnet.privateendpoints.id
  sas_enabled                = true # Needed for LogPoint
  network_ruleset = {
    allowed_subnet_ids = [
      azurerm_subnet.vnetintegrations.id
    ]
  }
}

resource "azurerm_eventhub_namespace_authorization_rule" "logpoint" {
  name                = "logpoint"
  namespace_name      = module.evhns_audit.name
  resource_group_name = azurerm_resource_group.this.name

  listen = true
  send   = false
  manage = false
}
