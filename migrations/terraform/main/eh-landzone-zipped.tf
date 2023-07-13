#---- Eventhub Namespace 

module "eventhub_namespace_landzone" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/eventhub-namespace?ref=v12"

  name                            = "ehn-landzone"
  project_name                    = var.domain_name_short
  environment_short               = var.environment_short
  environment_instance            = var.environment_instance
  resource_group_name             = azurerm_resource_group.this.name
  location                        = azurerm_resource_group.this.location
  sku                             = "Standard"
  private_endpoint_subnet_id      = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  network_ruleset = {
    allowed_subnet_ids = [data.azurerm_key_vault_secret.snet_vnet_integration_id.value]
  }
}

#---- Eventhub

module "eventhub_landzone_zipped" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/eventhub?ref=v12"

  name                = "eh-landzone-zipped"
  namespace_name      = module.eventhub_namespace_landzone.name
  resource_group_name = azurerm_resource_group.this.name
  partition_count     = 6
  message_retention   = 7
  auth_rules          = [
    {
      name    = "eh-landzone-listener-connection-string"
      listen  = true
    }
  ]
}
