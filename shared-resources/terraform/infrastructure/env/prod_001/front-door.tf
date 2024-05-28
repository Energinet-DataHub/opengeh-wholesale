# The Front Door
resource "azurerm_cdn_frontdoor_profile" "this" {
  name                = "afd-${local.resources_suffix}"
  resource_group_name = azurerm_resource_group.this.name
  sku_name            = "Premium_AzureFrontDoor"
}

# Add the WAF policy to the Front Door domains
# Add new dynamic block if another resource type is added such as APIM, static web app, etc.
resource "azurerm_cdn_frontdoor_security_policy" "this" {
  name                     = "fdfp-${local.resources_suffix}"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.this.id

  security_policies {
    firewall {
      cdn_frontdoor_firewall_policy_id = azurerm_cdn_frontdoor_firewall_policy.this.id

      association {
        dynamic "domain" {
          for_each = azurerm_cdn_frontdoor_custom_domain.this
          content {
            cdn_frontdoor_domain_id = domain.value.id
          }

        }
        dynamic "domain" {
          for_each = azurerm_cdn_frontdoor_custom_domain.ui
          content {
            cdn_frontdoor_domain_id = domain.value.id
          }

        }
        patterns_to_match = ["/*"]
      }
    }
  }
  depends_on = [azurerm_cdn_frontdoor_firewall_policy.this, azurerm_cdn_frontdoor_custom_domain.this, azurerm_cdn_frontdoor_custom_domain.ui]
}

resource "azurerm_monitor_diagnostic_setting" "front_door" {
  name                       = "mds-diagnostic-settings"
  target_resource_id         = azurerm_cdn_frontdoor_profile.this.id
  log_analytics_workspace_id = module.log_workspace_shared.id

  enabled_log {
    category = "FrontDoorAccessLog"
  }

  enabled_log {
    category = "FrontDoorHealthProbeLog"
  }

  enabled_log {
    category = "FrontDoorWebApplicationFirewallLog"
  }

  metric {
    category = "AllMetrics"
    enabled  = false
  }
}
