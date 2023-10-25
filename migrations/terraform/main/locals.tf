locals {
  task_workflow_setup_trigger     = "workflow_setup_${uuid()}"
  alert_trigger_cron              = "18 6 2/12 * * ?"
  monitor_trigger_cron            = "0 0/5 * * * ?"
  resources_suffix                = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  datahub2_certificate_thumbprint = ""
}
