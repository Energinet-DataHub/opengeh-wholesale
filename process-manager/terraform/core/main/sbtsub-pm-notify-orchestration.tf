module "sbtsub_pm_notify_orchestration" {
  source             = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=service-bus-topic-subscription_6.0.1"
  name               = "pm-notify-orchestration"
  topic_id           = data.azurerm_key_vault_secret.sbt_processmanager_id.value
  project_name       = var.domain_name_short
  max_delivery_count = 10

  correlation_filter = {
    label = "NotifyOrchestration" # Applies filter to the "subject" of the service bus message. IMPORTANT: This is case sensitive.
  }
}
