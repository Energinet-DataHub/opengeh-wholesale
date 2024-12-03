resource "azurerm_monitor_autoscale_setting" "apim_autoscale" {
  # Only create autoscale for preprod and prod, as that is the only places where it is premium
  count = var.enable_autoscale_apim ? 1 : 0

  name                = "apim-autoscale"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

  target_resource_id = module.apim_shared.id

  # Based on recommendations from https://learn.microsoft.com/en-us/azure/api-management/api-management-howto-autoscale
  profile {
    name = "AutoScaleApimProfile"

    capacity {
      minimum = 1 # Minimum instances
      maximum = 4 # Maximum instances
      default = 1 # Default instances
    }

    rule {
      metric_trigger {
        metric_name        = "Capacity"
        metric_resource_id = module.apim_shared.id
        operator           = "GreaterThan"
        threshold          = 70
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT30M"
        time_aggregation   = "Average"
      }

      scale_action {
        direction = "Increase"
        type      = "ChangeCount"
        value     = "1"
        cooldown  = "PT1H"
      }
    }

    rule {
      metric_trigger {
        metric_name        = "Capacity"
        metric_resource_id = module.apim_shared.id
        operator           = "LessThan"
        threshold          = 35
        time_grain         = "PT1M"
        statistic          = "Average"
        time_window        = "PT30M"
        time_aggregation   = "Average"
      }

      scale_action {
        direction = "Decrease"
        type      = "ChangeCount"
        value     = "1"
        cooldown  = "PT1H30M"
      }
    }
  }

  tags = local.tags
}
