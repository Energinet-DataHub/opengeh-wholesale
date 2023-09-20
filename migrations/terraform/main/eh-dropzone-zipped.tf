#---- Eventhub Namespace

resource "azurerm_eventhub_namespace" "eventhub_namespace_dropzone" {
  name                = "evhns-dropzone-${local.resources_suffix}"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  sku                 = "Standard"
  identity {
    type = "SystemAssigned"
  }
}

#---- Eventhub

resource "azurerm_eventhub" "eventhub_dropzone_zipped" {
  name                = "evh-dropzonezipped-${local.resources_suffix}"
  namespace_name      = azurerm_eventhub_namespace.eventhub_namespace_dropzone.name
  resource_group_name = azurerm_resource_group.this.name
  partition_count     = 10
  message_retention   = 7
}

#----- Event Hub Consumer Group

resource "azurerm_eventhub_consumer_group" "consumer_group_dropzone_zipped" {
  name                = "evhcg-dropzone-zipped-${local.resources_suffix}"
  namespace_name      = azurerm_eventhub_namespace.eventhub_namespace_dropzone.name
  eventhub_name       = azurerm_eventhub.eventhub_dropzone_zipped.name
  resource_group_name = azurerm_resource_group.this.name
}
