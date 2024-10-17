locals {
  job_names = [
    "Schema_Migration",
    "Bulk_Charge_Links_Landing_to_Gold",
    "Bulk_Charges_Landing_to_Gold",
    "duplicates_in_time_series_eloverblik",
    "monitor_eloverblik_time_series",
    "duplicates_in_metering_points_gold",
    "Gold_To_Eloverblik",
    "Gold_To_Wholesale",
    "Bulk_Time_Series_to_Bronze",
    "optimize_bronze_time_series",
    "Reprocess_Metering_Points_Silver_Quarantine",
    "Reprocess_Time_Series_Silver_Quarantine",
    "duplicates_in_time_series_silver",
    "Syncronize_To_Bronze_Time_Series",
    "Stream_Time_Series_Bronze_To_Silver",
    "Stream_Time_Series_Silver_To_Gold",
    "duplicates_in_metering_points_wholesale",
    "monitor_wholesale_metering_points",
    "monitor_wholesale_time_series",
    "Reprocess_Metering_Points_Bronze_Quarantine",
    "alert_bronze_vs_audit_differences",
    "alert_bronze_quarantined_ch_increased",
    "alert_bronze_quarantined_mp_increased",
    "alert_bronze_quarantined_ts_increased",
    "alert_silver_quarantined_ch_price_increased",
    "alert_silver_quarantined_mp_increased",
    "alert_silver_quarantined_ts_increased",
    "Ready_For_Calculation_Balance_Fixing",
    "Alert_Gold_Charge_Links_Changed",
    "Alert_Gold_Metering_Points_Changed",
    "Alert_Gold_Time_Series_Changed",
    "Bulk_Consumption_Statement_to_Bronze",
    "Bulk_Charge_Links_Gold_To_Wholesale",
    "Bulk_Charge_Links_Landing_to_Wholesale",
    "Bulk_Charges_Gold_To_Wholesale",
    "Bulk_Charges_Landing_to_Wholesale",
    "Bulk_Metering_Point_Landing_To_Wholesale",
    "Custom Operations",
    "Permanently_Quarantine_Silver_Time_Series",
    "Reprocess_Time_Series_Bronze_Quarantine",
    "Stream_Time_Series_Audit_Autoloader",
    "Stream_Time_Series_Gold_To_Eloverblik",
    "Stream_Time_Series_Gold_To_Wholesale"
  ]
}

data "databricks_jobs" "this" {
  provider = databricks.dbw

  depends_on = [module.dbw]
}

import {
  to = databricks_job.this["Bulk_Consumption_Statement_to_Bronze"]
  id = data.databricks_jobs.this.ids["Bulk_Consumption_Statement_to_Bronze"]
}

import {
  to = databricks_job.this["Bulk_Charge_Links_Gold_To_Wholesale"]
  id = data.databricks_jobs.this.ids["Bulk_Charge_Links_Gold_To_Wholesale"]
}

import {
  to = databricks_job.this["Bulk_Charge_Links_Landing_to_Wholesale"]
  id = data.databricks_jobs.this.ids["Bulk_Charge_Links_Landing_to_Wholesale"]
}

import {
  to = databricks_job.this["Bulk_Charges_Gold_To_Wholesale"]
  id = data.databricks_jobs.this.ids["Bulk_Charges_Gold_To_Wholesale"]
}

import {
  to = databricks_job.this["Bulk_Charges_Landing_to_Wholesale"]
  id = data.databricks_jobs.this.ids["Bulk_Charges_Landing_to_Wholesale"]
}

import {
  to = databricks_job.this["Bulk_Metering_Point_Landing_To_Wholesale"]
  id = data.databricks_jobs.this.ids["Bulk_Metering_Point_Landing_To_Wholesale"]
}

import {
  to = databricks_job.this["Custom Operations"]
  id = data.databricks_jobs.this.ids["Custom Operations"]
}

import {
  to = databricks_job.this["Permanently_Quarantine_Silver_Time_Series"]
  id = data.databricks_jobs.this.ids["Permanently_Quarantine_Silver_Time_Series"]
}

import {
  to = databricks_job.this["Reprocess_Time_Series_Bronze_Quarantine"]
  id = data.databricks_jobs.this.ids["Reprocess_Time_Series_Bronze_Quarantine"]
}

import {
  to = databricks_job.this["Stream_Time_Series_Audit_Autoloader"]
  id = data.databricks_jobs.this.ids["Stream_Time_Series_Audit_Autoloader"]
}

import {
  to = databricks_job.this["Stream_Time_Series_Gold_To_Eloverblik"]
  id = data.databricks_jobs.this.ids["Stream_Time_Series_Gold_To_Eloverblik"]
}

import {
  to = databricks_job.this["Stream_Time_Series_Gold_To_Wholesale"]
  id = data.databricks_jobs.this.ids["Stream_Time_Series_Gold_To_Wholesale"]
}
