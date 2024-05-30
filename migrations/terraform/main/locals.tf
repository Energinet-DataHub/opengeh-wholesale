locals {
  task_workflow_setup_trigger     = "workflow_setup_${uuid()}"
  alert_trigger_cron              = "18 6 2/12 * * ?"
  monitor_trigger_cron            = "0 0/5 * * * ?"
  resources_suffix                = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  datahub2_certificate_thumbprint = ""
  ip_restrictions_as_string       = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])
  databricks_runtime_version      = "14.3.x-scala2.12"

  # Logging (worker)
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_MIGRATIONS        = "Information" # From opengeh-migration
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE              = "Information" # From geh-core
  LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT                             = "Warning"     # Everything else

  # Logging (host)
  AZUREFUNCTIONSJOBHOST_LOGGING_LOGLEVEL_DEFAULT                   = "Warning"
  AZUREFUNCTIONSJOBHOST_LOGGING_LOGLEVEL_DURABLETASK_CORE          = "Warning"
  AZUREFUNCTIONSJOBHOST_LOGGING_LOGLEVEL_DURABLETASK_AZURESTORAGE  = "Warning"
  AZUREFUNCTIONSJOBHOST_LOGGING_LOGLEVEL_HOST_TRIGGERS_DURABLETASK = "Warning"
  LOGGING_APPINSIGHTS_SAMPLINGSETTINGS_ISENABLED                   = true
  LOGGING_APPINSIGHTS_SAMPLINGSETTINGS_EXCLUDEDTYPES               = "Request"

  # Logging - deprecated
  LOGGING_LOGLEVEL_DEFAULT                                         = "Warning"
  LOGGING_LOGLEVEL_WORKER_DEFAULT                                  = "Warning"
  LOGGING_LOGLEVEL_HOST_DEFAULT                                    = "Warning"
}
