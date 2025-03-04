module "sbtsub_pmnotify_notify_orchestration" {
  source             = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=service-bus-topic-subscription_7.0.0"
  name               = "pmnotify-notify-orchestration"
  topic_id           = data.azurerm_key_vault_secret.sbt_processmanagernotify_id.value
  project_name       = var.domain_name_short
  max_delivery_count = 10

  correlation_filter = {
    label = "NotifyOrchestration" # Applies filter to the "subject" of the service bus message. IMPORTANT: This is case sensitive.
  }
}
