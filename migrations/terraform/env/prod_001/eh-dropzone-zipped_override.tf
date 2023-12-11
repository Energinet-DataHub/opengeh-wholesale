#---- Eventhub Namespace

resource "azurerm_eventhub_namespace" "eventhub_namespace_dropzone" {
  capacity = 5
}
