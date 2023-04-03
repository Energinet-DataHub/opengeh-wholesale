locals {
    task_workflow_setup_trigger = "workflow_setup_${uuid()}"
    alert_trigger_cron = "18 6 2/12 * * ?"
    alert_job_email_notification = "36217b88.energinet.onmicrosoft.com@emea.teams.ms" # Volt's MS Teams channel
}

