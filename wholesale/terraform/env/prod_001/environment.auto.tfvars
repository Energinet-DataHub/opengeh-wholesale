# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/terraform.tfvars
databricks_vnet_address_space                      = "10.146.104.0/22"
databricks_private_subnet_address_prefix           = "10.146.104.0/24"
databricks_public_subnet_address_prefix            = "10.146.105.0/24"
databricks_private_endpoints_subnet_address_prefix = "10.146.106.0/24"
alert_email_address                                = "35dde102.energinet.onmicrosoft.com@emea.teams.ms"
pim_sql_writer_ad_group_name                       = "SEC-A-Datahub-Prod-001-Contributor"
pim_sql_reader_ad_group_name                       = "SEC-A-Datahub-Prod-001-Reader"
