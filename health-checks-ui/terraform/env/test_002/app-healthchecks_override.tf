module "app_health_checks" {
  app_settings = merge(local.default_app_health_checks_app_settings, {
    "HealthChecksUI__HealthChecks__025__Name" = "sauron:::func-github-api"
    "HealthChecksUI__HealthChecks__025__Uri"  = "https://func-github-api-sauron-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    "HealthChecksUI__HealthChecks__030__Name" = "sauron:::func-bff"
    "HealthChecksUI__HealthChecks__030__Uri"  = "https://func-bff-api-sauron-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    # Live - prefix with 2xx
    "HealthChecksUI__HealthChecks__225__Name" = "sauron:::func-github-api - live"
    "HealthChecksUI__HealthChecks__225__Uri"  = "https://func-github-api-sauron-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
    "HealthChecksUI__HealthChecks__230__Name" = "sauron:::func-bff - live"
    "HealthChecksUI__HealthChecks__230__Uri"  = "https://func-bff-api-sauron-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
  })
}
