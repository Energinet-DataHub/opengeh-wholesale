locals {
  task_workflow_setup_trigger     = "workflow_setup_${uuid()}"
  alert_trigger_cron              = "18 6 2/12 * * ?"
  monitor_trigger_cron            = "0 0/5 * * * ?"
  resources_suffix                = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  datahub2_certificate_thumbprint = ""
  ip_restrictions_as_string       = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])
  databricks_runtime_version      = "15.4.x-scala2.12"

  # Remove sensitive data from storage account
  remove_sensitive_data = false

  # Logging (worker)
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_MIGRATIONS = "Information" # From opengeh-migration
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE       = "Information" # From geh-core
  LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT                      = "Warning"     # Everything else

  # Logging (host)
  AZUREFUNCTIONSJOBHOST_LOGGING_LOGLEVEL_DEFAULT                           = "Warning"
  AZUREFUNCTIONSJOBHOST_LOGGING_LOGLEVEL_DURABLETASK_CORE                  = "Warning"
  AZUREFUNCTIONSJOBHOST_LOGGING_LOGLEVEL_DURABLETASK_AZURESTORAGE          = "Warning"
  AZUREFUNCTIONSJOBHOST_LOGGING_LOGLEVEL_HOST_TRIGGERS_DURABLETASK         = "Warning"
  AZUREFUNCTIONSJOBHOST_LOGGING_APPINSIGHTS_SAMPLINGSETTINGS_ISENABLED     = true
  AZUREFUNCTIONSJOBHOST_LOGGING_APPINSIGHTS_SAMPLINGSETTINGS_EXCLUDEDTYPES = "Request"

  # Logging - deprecated
  LOGGING_LOGLEVEL_DEFAULT        = "Warning"
  LOGGING_LOGLEVEL_WORKER_DEFAULT = "Warning"
  LOGGING_LOGLEVEL_HOST_DEFAULT   = "Warning"

  tags = {
    "BusinessServiceName"   = "Datahub",
    "BusinessServiceNumber" = "BSN10136"
  }

  # Databricks permissions
  # Local readers determines if the provided reader security group should be assigned permissions or grants.
  # This is necessary as reader and contributor groups may be the same on the development and test environments. In Databricks, the grants and permissions of a security group can't be be managed by multiple resources.
  readers = var.databricks_readers_group.name == var.databricks_contributor_dataplane_group.name ? {} : { "${var.databricks_readers_group.name}" = "${var.databricks_readers_group.id}" }

  backup_access_control = local.readers == {} ? [
    {
      group_name         = var.databricks_contributor_dataplane_group.name
      contributor_access = true
    }] : [
    {
      group_name         = var.databricks_contributor_dataplane_group.name
      contributor_access = true
    },
    {
      group_name         = var.databricks_readers_group.name
      contributor_access = false
  }]
}
