# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
databricks_vnet_address_space                      = "10.142.100.0/22"
databricks_private_subnet_address_prefix           = "10.142.100.0/24"
databricks_public_subnet_address_prefix            = "10.142.101.0/24"
databricks_private_endpoints_subnet_address_prefix = "10.142.102.0/24"
datahub2_ip_whitelist                              = "86.106.96.1"
feature_flag_datahub2_healthcheck                  = false
