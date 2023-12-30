resource "azurerm_monitor_scheduled_query_rules_alert" "wholesale_calculator_job_alert" {
  name                = "alert-calculator-job-failed-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  location            = azurerm_resource_group.this.location
  resource_group_name = data.azurerm_resource_group.shared.name

  action {
    action_group = [data.azurerm_key_vault_secret.primary_action_group_id.value]
  }

  data_source_id = data.azurerm_key_vault_secret.log_shared_id.value
  description    = "One or more calculation jobs has failed"
  # Currently disabled as some failures are expected and we don't want false alerts
  # Besides, the batch overview page in the front-end shows failed status
  enabled     = false
  query       = <<-QUERY
  DatabricksJobs
| where OperationName == "Microsoft.Databricks/jobs/runFailed"
| where parse_json(RequestParams).taskKey startswith "calculator_job"
  QUERY
  severity    = 1
  frequency   = 5
  time_window = 5
  trigger {
    operator  = "GreaterThan"
    threshold = 0
  }
}
