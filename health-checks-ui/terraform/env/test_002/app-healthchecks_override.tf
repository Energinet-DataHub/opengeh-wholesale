module "app_health_checks" {
  app_settings = merge(local.default_app_health_checks_app_settings, {
    "HealthChecksUI__HealthChecks__025__Name" = "sauron:::func-githubapi"
    "HealthChecksUI__HealthChecks__025__Uri"  = "https://func-githubapi-sauron-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/ready"
    # Live - prefix with 2xx
    "HealthChecksUI__HealthChecks__025__Name" = "sauron:::func-githubapi - live"
    "HealthChecksUI__HealthChecks__025__Uri"  = "https://func-githubapi-sauron-${lower(var.environment_short)}-we-${lower(var.environment_instance)}.azurewebsites.net/api/monitor/live"
  })
}
