locals {
  integration_mssqlserver_admin_name = "inttestdbadmin"
  resource_suffix_with_dash          = "${lower(var.domain_name_short)}-${lower(var.environment_short)}-we-${lower(var.environment_instance)}"
  resource_suffix_without_dash       = "${lower(var.domain_name_short)}${lower(var.environment_short)}we${lower(var.environment_instance)}"
  databricks_runtime_version         = "15.4.x-scala2.12"
  databricks_unity_catalog_name      = replace(azurerm_databricks_workspace.this.name, "-", "_") # Catalog created from default is workspace name with dashes replaced by underscores

  tags = {
    "BusinessServiceName"   = "Datahub",
    "BusinessServiceNumber" = "BSN10136"
  }

  b2c_backend_app_roles = {
    balanceresponsibleparty = {
      role         = "balanceresponsibleparty"
      display_name = "Balance Responsible Party"
    }
    gridaccessprovider = {
      role         = "gridaccessprovider"
      display_name = "Grid Access Provider"
    }
    billingagent = {
      role         = "billingagent"
      display_name = "Billing Agent"
    }
    energysupplier = {
      role         = "energysupplier"
      display_name = "Energy Supplier"
    }
    independentaggregator = {
      role         = "independentaggregator"
      display_name = "Independent Aggregator"
    }
    systemoperator = {
      role         = "systemoperator"
      display_name = "System Operator"
    }
    danishenergyagency = {
      role         = "danishenergyagency"
      display_name = "Danish Energy Agency"
    }
    dataHubadministrator = {
      role         = "dataHubadministrator"
      display_name = "Datahub Administrator"
    }
    imbalancesettlementresponsible = {
      role         = "imbalancesettlementresponsible"
      display_name = "Imbalance Settlement Responsible"
    }
    metereddataadministrator = {
      role         = "metereddataadministrator"
      display_name = "Metered Data Administrator"
    }
    metereddataresponsible = {
      role         = "metereddataresponsible"
      display_name = "Metered Data Responsible"
    }
    meteringpointadministrator = {
      role         = "meteringpointadministrator"
      display_name = "Meteringpoint Administrator"
    }
    serialenergytrader = {
      role         = "serialenergytrader"
      display_name = "Serial Energy Trader"
    }
    meteroperator = {
      role         = "meteroperator"
      display_name = "Serial Energy Trader"
    }
    delegated = {
      role         = "delegated"
      display_name = "Delegated"
    }
    itsupplier = {
      role         = "itsupplier"
      display_name = "IT Supplier"
    }
  }
}
