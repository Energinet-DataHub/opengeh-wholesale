module "sbtsub_settlement_reports_event_listener" {
  source             = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=service-bus-topic-subscription_7.0.0"
  name               = "settlement-reports"
  topic_id           = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_id.value
  project_name       = var.domain_name_short
  max_delivery_count = 10
  sql_filter         = { name = "integration-event-filter", filter = "sys.label = 'SettlementReportJobFinished' or sys.label = 'GridAreaOwnershipAssigned'" }
}
