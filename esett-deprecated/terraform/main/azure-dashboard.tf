data "azurerm_subscription" "current" {}

data "template_file" "dash-template" {
  template  = "${file("${path.module}/template-azure-dashboard.json")}"
  vars      = {
    subscription_id           = data.azurerm_subscription.current.subscription_id
    resource_group_name       = azurerm_resource_group.this.name
    application_insight_name  = data.azurerm_key_vault_secret.appi_shared_name.value
  }
}

resource "azurerm_dashboard" "main" {
  name                  = "azure-dashboard-${local.name_suffix}"
  resource_group_name   = azurerm_resource_group.this.name
  location              = azurerm_resource_group.this.location
  dashboard_properties  = data.template_file.dash-template.rendered
}
