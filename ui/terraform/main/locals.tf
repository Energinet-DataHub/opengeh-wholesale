locals {
  frontend_url = var.frontend_url != null ? var.frontend_url : azurerm_static_site.ui.default_host_name
}
