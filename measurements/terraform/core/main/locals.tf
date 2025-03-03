locals {
  # IP restrictions
  ip_restrictions_as_string = join(",", [for rule in var.ip_restrictions : "${rule.ip_address}"])

  resource_suffix_with_dash = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  tags = {
    "BusinessServiceName"   = "Datahub",
    "BusinessServiceNumber" = "BSN10136"
  }

  # Databricks permissions
  # Local readers determines if the provided reader security group should be assigned permissions or grants.
  # This is necessary as reader and contributor groups may be the same on the development and test environments. In Databricks, the grants and permissions of a security group can't be be managed by multiple resources.
  readers = var.databricks_readers_group.name == var.databricks_contributor_dataplane_group.name ? {} : { "${var.databricks_readers_group.name}" = "${var.databricks_readers_group.id}" }

  ################################## Electrical heating ##################################

  # Databricks runtime version for jobs
  # Python version for "15.4.x-scala2.12" is 3.11.0
  spark_version = "15.4.x-scala2.12"

  TIME_ZONE = "Europe/Copenhagen"

  ################################## Core ##################################

  ### .Net

  # Logging
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_MEASUREMENTS_CORE = "Information"
  LOGGING_APPINSIGHTS_LOGLEVEL_ENERGINET_DATAHUB_CORE              = "Information"
  LOGGING_APPINSIGHTS_LOGLEVEL_DEFAULT                             = "Warning"

  # All Databricks jobs that should stream in our workspaces
  databricks_jobs_string = join(",", [
    databricks_job.bronze_submitted_transactions_to_silver.name,
    databricks_job.bronze_submitted_transactions_ingestion_stream.name,
    databricks_job.silver_to_gold_measurements.name,
    databricks_job.silver_notify_transactions_persisted_stream.name,
    databricks_job.bronze_migrate_transactions_batch_job.name
  ])
}
