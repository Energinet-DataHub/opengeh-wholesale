# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
alert_email_address                                                 = "3679b895.energinet.onmicrosoft.com@emea.teams.ms"
create_azure_load_testing_resource                                  = true
feature_management_receive_metered_data_for_measurement_points      = true
feature_management_use_peek_time_series_messages                    = true
feature_management_use_standard_blob_service_client                 = true

feature_management_use_request_wholesale_services_process_orchestration         = false
feature_management_use_request_aggregated_measure_data_process_orchestration    = false
feature_management_use_process_manager_to_enqueue_brs023027_messages            = true