resource "azuread_application" "frontend_app" {
  single_page_application {
    redirect_uris = ["https://${azurerm_static_site.ui.default_host_name}/", "https://localhost/"]
  }
}
