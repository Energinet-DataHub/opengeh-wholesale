resource "azurerm_dashboard_grafana" "this" {
  name                          = "amg-${local.resources_suffix}"
  resource_group_name           = azurerm_resource_group.this.name
  location                      = azurerm_resource_group.this.location
  grafana_major_version         = 10
  api_key_enabled               = true
  public_network_access_enabled = true

  smtp {
    enabled          = true
    user             = "apikey"
    password         = var.sendgrid_api_key
    start_tls_policy = "OpportunisticStartTLS"
    from_address     = "no-reply@datahub.dk"
    host             = "smtp.sendgrid.net:465"
  }

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

# Only create grafana reports on 001 environments
# Send report for the previous month at the first day of the month
resource "grafana_report" "datahub_overview" {
  count = var.environment_instance == "001" ? 1 : 0

  name       = "Report of DataHub Overview ${lower(var.environment)}_${lower(var.environment_instance)}"
  recipients = ["nhq@energinet.dk;xbspo@energinet.dk;tni@energinet.dk;mrk@energinet.dk"]
  formats    = ["pdf"]
  message    = "The monthly report PDF of the DataHub Overview dashboard for ${lower(var.environment)}_${lower(var.environment_instance)}"
  dashboards {
    uid = grafana_dashboard.datahub_overview.uid

    # https://grafana.com/docs/grafana/latest/dashboards/use-dashboards/#time-units-and-relative-ranges
    time_range {
      from = "now-1M/M"
      to   = "now-1M/M"
    }
    report_variables = {
      ds  = "azure-monitor-oob"
      sub = var.subscription_id
      rg  = azurerm_resource_group.this.name
      res = module.appi_shared.name
    }
  }
  schedule {
    timezone   = "Europe/Copenhagen"
    frequency  = "monthly"
    start_time = "2024-10-01T08:00:00"
  }

  depends_on = [azurerm_dashboard_grafana.this]
}
