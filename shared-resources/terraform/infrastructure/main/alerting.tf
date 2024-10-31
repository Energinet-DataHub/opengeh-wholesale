module "monitor_action_group_shres" {
  count = var.alert_email_address != null ? 1 : 0

  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/monitor-action-group-email?ref=monitor-action-group-email_6.0.1"

  name                 = "alerts"
  project_name         = var.domain_name_short
  environment_short    = var.environment_short
  environment_instance = var.environment_instance
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location

  short_name             = "sres-alerts"
  email_receiver_name    = "Shared Resources Operations"
  email_receiver_address = var.alert_email_address

  application_insights_id = module.appi_shared.id
}

# Critical alert when we hit 85% of quota
resource "azurerm_monitor_scheduled_query_rules_alert_v2" "quota" {
  count = var.alert_email_address != null ? 1 : 0

  name                 = "quota-alert-${local.resources_suffix}"
  display_name         = "quota-alert-${local.resources_suffix}"
  resource_group_name  = azurerm_resource_group.this.name
  location             = azurerm_resource_group.this.location
  description          = "Alerting on quotas"
  enabled              = true
  evaluation_frequency = "PT5M"
  window_duration      = "PT5M"
  scopes               = [module.log_workspace_shared.id]
  severity             = 0 # Critical

  criteria {
    query                   = <<-QUERY
      arg("").QuotaResources
      | where subscriptionId =~ '${var.subscription_id}'
      | where type =~ 'microsoft.compute/locations/usages'
      | where isnotempty(properties)
      | mv-expand propertyJson = properties.value limit 400
      | extend
          usage = propertyJson.currentValue,
          quota = propertyJson.['limit'],
          quotaName = tostring(propertyJson.['name'].value)
      | extend usagePercent = toint(usage)*100 / toint(quota)| project-away properties| where location in~ ('westeurope')
      QUERY
    time_aggregation_method = "Maximum"
    threshold               = 85
    operator                = "GreaterThanOrEqual"
    metric_measure_column   = "usagePercent"
    failing_periods {
      minimum_failing_periods_to_trigger_alert = 1
      number_of_evaluation_periods             = 1
    }
  }

  action {
    action_groups = [module.monitor_action_group_shres[0].id]
  }

  identity {
    type = "SystemAssigned"
  }

  tags = local.tags
}

# Assign roles to the query alert rule. It needs reader access to the subscription and to the log analytics workspace
resource "azurerm_role_assignment" "quota_alert_reader_subscription" {
  count = var.alert_email_address != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Reader"
  principal_id         = azurerm_monitor_scheduled_query_rules_alert_v2.quota[0].identity[0].principal_id
}

# Alert us when quota is 60, but also ensure we only get one alert until resolved by using auto_mitigation_enabled
resource "azurerm_monitor_scheduled_query_rules_alert_v2" "quota_60" {
  count = var.alert_email_address != null ? 1 : 0

  name                    = "quota-alert60-${local.resources_suffix}"
  display_name            = "quota-alert60-${local.resources_suffix}"
  resource_group_name     = azurerm_resource_group.this.name
  location                = azurerm_resource_group.this.location
  description             = "Alerting on quotas when it's 60 percent"
  enabled                 = true
  evaluation_frequency    = "PT1H"
  window_duration         = "PT1H"
  auto_mitigation_enabled = true
  scopes                  = [module.log_workspace_shared.id]
  severity                = 2 # Warning

  criteria {
    query                   = <<-QUERY
      arg("").QuotaResources
      | where subscriptionId =~ '${var.subscription_id}'
      | where type =~ 'microsoft.compute/locations/usages'
      | where isnotempty(properties)
      | mv-expand propertyJson = properties.value limit 400
      | extend
          usage = propertyJson.currentValue,
          quota = propertyJson.['limit'],
          quotaName = tostring(propertyJson.['name'].value)
      | extend usagePercent = toint(usage)*100 / toint(quota)| project-away properties| where location in~ ('westeurope')
      QUERY
    time_aggregation_method = "Maximum"
    threshold               = 60
    operator                = "GreaterThanOrEqual"
    metric_measure_column   = "usagePercent"
    failing_periods {
      minimum_failing_periods_to_trigger_alert = 1
      number_of_evaluation_periods             = 1
    }
  }

  action {
    action_groups = [module.monitor_action_group_shres[0].id]
  }

  identity {
    type = "SystemAssigned"
  }

  tags = local.tags
}

# Assign roles to the query alert rule. It needs reader access to the subscription and to the log analytics workspace
resource "azurerm_role_assignment" "quota_alert_reader_subscription_60" {
  count = var.alert_email_address != null ? 1 : 0

  scope                = data.azurerm_subscription.this.id
  role_definition_name = "Reader"
  principal_id         = azurerm_monitor_scheduled_query_rules_alert_v2.quota_60[0].identity[0].principal_id
}
