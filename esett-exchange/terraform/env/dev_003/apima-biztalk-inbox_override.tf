module "apim_biztalk_inbox" {
  count = 0
}

resource "azurerm_api_management_backend" "biztalk_inbox_backend" {
  count = 0
}

module "apimao_biztalk_inbox" {
  count = 0
}
