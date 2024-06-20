data "azuread_client_config" "current" {
  provider = azuread.b2c
}
data "azuread_application_published_app_ids" "well_known" {
  provider = azuread.b2c
}
data "azurerm_client_config" "current" {}
