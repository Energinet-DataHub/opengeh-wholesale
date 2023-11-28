locals {
  b2b_id_uri           = "https://${var.b2c_tenant_name}.onmicrosoft.com/backend-b2b"
  bff_id_uri           = "https://${var.b2c_tenant_name}.onmicrosoft.com/backend-bff"
  timeseriesapi_id_uri = "https://${var.b2c_tenant_name}.onmicrosoft.com/backend-timeseriesapi"

  app_roles = {
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
  }

  timeseriesapi_app_roles = {
    eloverblik = {
      role         = "eloverblik"
      display_name = "ElOverblik"
    }
  }
}
