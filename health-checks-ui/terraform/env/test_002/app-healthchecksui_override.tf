module "app_health_checks_ui" {
  app_settings = merge(local.default_app_health_checks_ui_app_settings, {
    "HealthChecksUI__HealthChecks__014__Name" = "esett-deprecated:::Importer Function"
    "HealthChecksUI__HealthChecks__014__Uri"  = "https://func-dh2importer-esettdepr-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__015__Name" = "esett-deprecated:::BiztalkShipper Function"
    "HealthChecksUI__HealthChecks__015__Uri"  = "https://func-biztalkshipper-esettdepr-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__016__Name" = "esett-deprecated:::Receiver Function"
    "HealthChecksUI__HealthChecks__016__Uri"  = "https://func-biztalkreceiver-esettdepr-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__017__Name" = "esett-deprecated:::ChangeObserver Function"
    "HealthChecksUI__HealthChecks__017__Uri"  = "https://func-changeobserver-esettdepr-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__018__Name" = "esett-deprecated:::Converter Function"
    "HealthChecksUI__HealthChecks__018__Uri"  = "https://func-converter-esettdepr-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__018__Name" = "esett-deprecated:::EnrichmentIngestor Function"
    "HealthChecksUI__HealthChecks__018__Uri"  = "https://func-enrichmentingestor-esettdepr-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    # Live - prefix with 2xx
    "HealthChecksUI__HealthChecks__214__Name" = "esett-deprecated:::Importer Function - live"
    "HealthChecksUI__HealthChecks__214__Uri"  = "https://func-dh2importer-esettdepr-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__215__Name" = "esett-deprecated:::BiztalkShipper Function - live"
    "HealthChecksUI__HealthChecks__215__Uri"  = "https://func-biztalkshipper-esettdepr-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__216__Name" = "esett-deprecated:::Receiver Function - live"
    "HealthChecksUI__HealthChecks__216__Uri"  = "https://func-biztalkreceiver-esettdepr-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__217__Name" = "esett-deprecated:::ChangeObserver Function - live"
    "HealthChecksUI__HealthChecks__217__Uri"  = "https://func-changeobserver-esettdepr-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__218__Name" = "esett-deprecated:::Converter Function - live"
    "HealthChecksUI__HealthChecks__218__Uri"  = "https://func-converter-esettdepr-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__218__Name" = "esett-deprecated:::EnrichmentIngestor Function - live"
    "HealthChecksUI__HealthChecks__218__Uri"  = "https://func-enrichmentingestor-esettdepr-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
  })
}
