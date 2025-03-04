module "sbtsub_pmstart_brs_028" {
  source             = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=service-bus-topic-subscription_7.0.0"
  name               = "pmstart-brs-028"
  topic_id           = data.azurerm_key_vault_secret.sbt_processmanagerstart_id.value
  project_name       = var.domain_name_short
  max_delivery_count = 10

  correlation_filter = {
    label = "Brs_028" # Applies filter to the "subject" of the service bus message. IMPORTANT: This is case sensitive.
  }
}
