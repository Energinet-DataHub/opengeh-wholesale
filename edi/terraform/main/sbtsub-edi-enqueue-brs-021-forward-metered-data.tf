module "sbtsub_edi_enqueue_brs_021_forward_metered_data" {
  source             = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=service-bus-topic-subscription_6.0.1"
  name               = "edi-enqueue-brs-021-forward-metered-data"
  topic_id           = data.azurerm_key_vault_secret.sbt_edi_id.value
  project_name       = var.domain_name_short
  max_delivery_count = 10

  correlation_filter = {
    label = "Enqueue_brs_021_forward_metered_data" # Applies filter to the "subject" of the service bus message. IMPORTANT: This is case sensitive.
  }
}
