module "func_service_plan" {
  cpu_alert_information = {
    alerts_enabled = length(module.monitor_action_group_mig) != 1 ? false : true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 90
    severity       = 2
  }

  memory_alert_information = {
    alerts_enabled = length(module.monitor_action_group_mig) != 1 ? false : true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 90
    severity       = 2
  }
}

module "message_retriever_service_plan" {
  cpu_alert_information = {
    alerts_enabled = length(module.monitor_action_group_mig) != 1 ? false : true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 90
    severity       = 2
  }

  memory_alert_information = {
    alerts_enabled = length(module.monitor_action_group_mig) != 1 ? false : true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 90
    severity       = 2
  }
}

module "message_processor_service_plan" {
  cpu_alert_information = {
    alerts_enabled = length(module.monitor_action_group_mig) != 1 ? false : true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 90
    severity       = 2
  }

  memory_alert_information = {
    alerts_enabled = length(module.monitor_action_group_mig) != 1 ? false : true
    frequency      = "PT1M"
    window_size    = "PT5M"
    threshold      = 90
    severity       = 2
  }
}
