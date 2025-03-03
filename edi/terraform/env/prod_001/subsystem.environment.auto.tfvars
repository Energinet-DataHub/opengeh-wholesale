# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
alert_email_address                  = "4f683395.energinet.onmicrosoft.com@emea.teams.ms"
budget_alert_amount                  = 5800 # See issue 2359

feature_management_use_peek_messages = true

# Requests using Process Manager
feature_management_use_request_wholesale_services_process_orchestration         = true
feature_management_use_request_aggregated_measure_data_process_orchestration    = true
feature_management_use_process_manager_to_enqueue_brs023027_messages            = true