# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
databricks_vnet_address_space="10.142.100.0/22"
databricks_private_subnet_address_prefix="10.142.100.0/24"
databricks_public_subnet_address_prefix="10.142.101.0/24"
databricks_private_endpoints_subnet_address_prefix="10.142.102.0/24"
developer_object_ids = [
    "c2f75d02-f64f-4111-aaea-6c43f5bc8d65",
    "b3e348db-659d-46a2-bee3-942ceb1dcba4"
]
datahub2_ip_whitelist="86.106.96.1"
feature_flag_datahub2_healthcheck=false
