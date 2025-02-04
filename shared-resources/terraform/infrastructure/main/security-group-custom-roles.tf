// There is no built-in role for reading appsettings :o/
// Reference: https://github.com/MicrosoftDocs/azure-docs/issues/59847#issuecomment-871298764
resource "azurerm_role_definition" "app_config_settings_read_access" {
  name        = "datahub-app-config-settings-read-access-${var.environment_short}-${local.region_code}-${var.environment_instance}"
  scope       = data.azurerm_subscription.this.id
  description = "Allow reading config settings in Function apps and App Services"

  permissions {
    actions = [
      "Microsoft.Web/sites/config/list/Action",
      "Microsoft.Web/sites/config/Read"
    ]
  }
}

# There is no built-in role for managing APIM groups
resource "azurerm_role_definition" "apim_groups_contributor_access" {
  name        = "datahub-apim-groups-contributor-access-${var.environment_short}-${local.region_code}-${var.environment_instance}"
  scope       = data.azurerm_subscription.this.id
  description = "Allow adding and removing APIM users to APIM groups"

  permissions {
    actions = [
      "Microsoft.ApiManagement/service/groups/*"
    ]
  }
}

# There is no built-in role for managing alerts without giving many other permissions
resource "azurerm_role_definition" "alertsmanager" {
  name        = "datahub-alertsmanager-${var.environment_short}-${local.region_code}-${var.environment_instance}"
  scope       = data.azurerm_subscription.this.id
  description = "Allow management of alerts"

  permissions {
    actions = [
      "Microsoft.AlertsManagement/alerts/read",
      "Microsoft.AlertsManagement/alerts/history/read",
      "Microsoft.AlertsManagement/alerts/changestate/action"
    ]
  }
}

# There is no built-in role for managing locks without giving many other permissions
resource "azurerm_role_definition" "locks_contributor_access" {
  name        = "datahub-locks-contributor-access-${var.environment_short}-${local.region_code}-${var.environment_instance}"
  scope       = data.azurerm_subscription.this.id
  description = "Allow management and deletion of locks"

  permissions {
    actions = [
      "Microsoft.Authorization/locks/*"
    ]
  }
}

resource "azurerm_role_definition" "contributor_app_developers" {
  name        = "datahub-app-manage-contributor-access-${var.environment_short}-${local.region_code}-${var.environment_instance}"
  scope       = data.azurerm_subscription.this.id
  description = "Allow restarting, stopping, and starting Function apps and App Services"

  permissions {
    actions = [
      "Microsoft.Web/sites/restart/Action",
      "Microsoft.Web/sites/stop/Action",
      "Microsoft.Web/sites/start/Action",
      "Microsoft.Web/sites/slots/restart/Action",
      "Microsoft.Web/sites/slots/start/Action",
      "Microsoft.Web/sites/slots/stop/Action",
      "Microsoft.Web/sites/config/list/Action",
      "Microsoft.Web/sites/config/Read",
      "microsoft.web/sites/extensions/*",
      "microsoft.web/sites/slots/extensions/*"
    ]
  }
}

resource "azurerm_role_definition" "sql_db_query_performance_insight_reader" {
  name        = "datahub-sql-db-query-performance-insight-reader-${var.environment_short}-${local.region_code}-${var.environment_instance}"
  scope       = data.azurerm_subscription.this.id
  description = "Allows access to Query Performance Insight details of SQL Databases without full Contributor permissions."

  permissions {
    actions = [
      "Microsoft.Sql/servers/databases/read",
      "Microsoft.Insights/metrics/read",
      "Microsoft.Sql/servers/databases/queryStore/read",
      "Microsoft.Sql/servers/databases/queryStore/queryTexts/read",
      "Microsoft.Sql/servers/databases/topQueries/queryText/action"
    ]
  }
}
