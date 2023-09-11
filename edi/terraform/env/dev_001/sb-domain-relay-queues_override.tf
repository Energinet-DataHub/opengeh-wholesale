module "sbq_incoming_change_supplier_messagequeue" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=v13"
}

module "sbq_incoming_aggregated_measure_data_messagequeue" {
  source = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-queue?ref=v13"
}
