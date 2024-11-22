resource "grafana_data_source" "github" {
  name = "GitHub (App=Datahub - Grafana Datasource)"
  type = "grafana-github-datasource"

  json_data_encoded = jsonencode({
    # When Azure Managed Grafana supports version 1.9 of github-datasource, we can switch to app authentication
    # "selectedAuthType" = "github-app"
    "selectedAuthType" = "personal-access-token"
    "appId"            = var.dh_grafana_datasource_app_id
    "installationId"   = var.dh_grafana_datasource_installation_id
    "owner"            = "Energinet-DataHub"
  })

  secure_json_data_encoded = jsonencode({
    "accessToken" = var.dh_grafana_datasource_personal_access_token
    # When Azure Managed Grafana supports version 1.9 of github-datasource, we can switch to app authentication
    # "privateKey" = base64decode(var.dh_grafana_datasource_privatekey_base64)
  })
}

data "template_file" "github_cd_overview_json" {
  template = file("${path.module}/dashboards/github_cd_overview.json")
  vars = {
    resource_group = azurerm_resource_group.this.name
    subscription   = var.subscription_id
    env_short      = var.environment_short
    tenant         = var.arm_tenant_id
    environment    = var.environment
    instance       = var.environment_instance
  }
}

resource "grafana_dashboard" "github_cd_overview" {
  config_json = data.template_file.github_cd_overview_json.rendered
}
