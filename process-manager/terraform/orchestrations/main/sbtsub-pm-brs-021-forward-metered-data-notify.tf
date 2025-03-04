module "sbtsub_pm_brs_021_forward_metered_data_notify" {
  source             = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=service-bus-topic-subscription_7.0.0"
  name               = "pm-brs-021-forward-metered-data-notify"
  topic_id           = data.azurerm_key_vault_secret.sbt_brs021forwardmetereddatanotify_id.value
  project_name       = var.domain_name_short
  max_delivery_count = 10
}
