resource "azurerm_dashboard_grafana" "this" {
  name                          = "amg-${local.resources_suffix}"
  resource_group_name           = azurerm_resource_group.this.name
  location                      = azurerm_resource_group.this.location
  grafana_major_version         = 10
  api_key_enabled               = true
  public_network_access_enabled = true

  identity {
    type = "SystemAssigned"
  }

  tags = local.tags
}

resource "azurerm_role_assignment" "grafana_subscription" {
  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Reader"
  principal_id         = azurerm_dashboard_grafana.this.identity[0].principal_id
}

resource "azurerm_role_assignment" "spn_grafana" {
  scope                = azurerm_dashboard_grafana.this.id
  role_definition_name = "Grafana Admin"
  principal_id         = data.azurerm_client_config.current.object_id
}

# Developers RBAC
resource "azurerm_role_assignment" "grafana_developers" {
  scope                = azurerm_dashboard_grafana.this.id
  role_definition_name = var.developer_security_group_contributor_access ? "Grafana Editor" : "Grafana Viewer"
  principal_id         = data.azuread_group.developer_security_group_name.object_id
}

# Platform Developers RBAC
resource "azurerm_role_assignment" "grafana_platform" {
  scope                = azurerm_dashboard_grafana.this.id
  role_definition_name = var.platform_security_group_contributor_access ? "Grafana Admin" : "Grafana Viewer"
  principal_id         = data.azuread_group.platform_security_group_name.object_id
}

# Key expires once a year
resource "time_rotating" "grafana" {
  rotation_days = 340
}

resource "shell_script" "create_key" {
  lifecycle_commands {
    create = file("${path.module}/scripts/grafana.sh")
    delete = "echo 'new key to be created'"
  }

  environment = {
    grafanaName   = "${azurerm_dashboard_grafana.this.name}"
    subscription  = "${var.subscription_id}"
    resourceGroup = "${azurerm_resource_group.this.name}"
  }

  interpreter = ["/bin/bash", "-c"]

  triggers = {
    grafanaName   = "${azurerm_dashboard_grafana.this.name}"
    subscription  = "${var.subscription_id}"
    resourceGroup = "${azurerm_resource_group.this.name}"
    timeRotating  = "${time_rotating.grafana.rotation_days}"
  }

  depends_on = [azurerm_role_assignment.spn_grafana]
}

data "template_file" "dashboard_json" {
  template = file("${path.module}/dashboards/datahub-overview.json")
  vars = {
    resource_group = azurerm_resource_group.this.name
    subscription   = var.subscription_id
    env_short      = var.environment_short
    tenant         = var.arm_tenant_id
    environment    = var.environment
    instance       = var.environment_instance
  }
}

# JSON file is placed in dashboards folder
resource "grafana_dashboard" "datahub_overview" {
  config_json = data.template_file.dashboard_json.rendered
}

