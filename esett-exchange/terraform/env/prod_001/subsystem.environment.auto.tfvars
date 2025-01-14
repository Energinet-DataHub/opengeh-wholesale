# This file contains values that are specific to this environment.
# For values that persist across all environments, refer to /main/subsystem.auto.tfvars
disable_biztalk_backoff  = true
biz_talk_sender_code     = "45V000000000056T"
biz_talk_receiver_code   = "44V000000000029A"
alert_email_address      = "194057a9.energinet.onmicrosoft.com@emea.teams.ms"
budget_alert_amount      = 6100 # See issue #2359
mssql_sku_name           = "GP_S_Gen5_6"
mssql_min_capacity_vcore = 1
mssql_max_size_gb        = 10
