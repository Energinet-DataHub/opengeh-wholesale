module "sbtsub_esett_exchange_event_listener" {
  sql_filter = { name = "integration-event-filter", filter = "sys.label = 'GridAreaOwnershipAssigned'" }
}
