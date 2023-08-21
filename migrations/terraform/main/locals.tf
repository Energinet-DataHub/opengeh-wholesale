locals {
  task_workflow_setup_trigger = "workflow_setup_${uuid()}"
  alert_trigger_cron = "18 6 2/12 * * ?"
  monitor_trigger_cron = "0 0/5 * * * ?"
}

