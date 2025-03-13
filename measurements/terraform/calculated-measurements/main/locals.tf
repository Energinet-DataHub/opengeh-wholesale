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
  
  # Database Names TODO: Currently these values are also hardcoded into unity.tf within the core project.
  # However that part needs to be migrated to calculated-measurements.
  database_measurements_calculated_internal = "measurements_calculated_internal"
  database_measurements_calculated = "measurements_calculated"
}
