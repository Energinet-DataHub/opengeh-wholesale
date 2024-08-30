resource "azurerm_cdn_frontdoor_firewall_policy" "this" {
  name                              = "waf${lower(var.domain_name_short)}${lower(var.environment_short)}we${lower(var.environment_instance)}"
  resource_group_name               = azurerm_resource_group.this.name
  sku_name                          = azurerm_cdn_frontdoor_profile.this.sku_name
  enabled                           = true
  mode                              = "Prevention"
  custom_block_response_status_code = 403
  custom_block_response_body        = "PGh0bWw+CjxoZWFkZXI+PHRpdGxlPkJsb2NrZWQ8L3RpdGxlPjwvaGVhZGVyPgo8Ym9keT4KQmxvY2tlZAo8L2JvZHk+CjwvaHRtbD4=" # Base64 encoded HTML content for a block page

  custom_rule {
    name     = "geo"
    enabled  = true
    priority = 1
    type     = "MatchRule"
    action   = "Block"

    match_condition {
      match_variable = "RemoteAddr"
      operator       = "GeoMatch"
      # Max 10 is allowed
      # See abbreviations here: https://learn.microsoft.com/en-us/azure/web-application-firewall/ag/geomatch-custom-rules
      # See why countries are added here: https://energinet.atlassian.net/wiki/spaces/D3/pages/816087043/Azure+Front+Door+and+Web+Application+Firewall
      # RU - Russia, BY - Belarus, IR - Iran, KP - North Korea, CN - China
      match_values = ["RU", "BY", "IR", "KP", "CN"]
    }
  }

  managed_rule {
    type    = "Microsoft_DefaultRuleSet"
    version = "2.1"
    action  = "Block"

    # Exclude cookie from evaluation, as WAF sees it as SQL injection
    exclusion {
      match_variable = "RequestCookieNames"
      operator       = "Equals"
      selector       = "CookieInformationConsent"
    }

    # Exclude GraphQL body payloads from evaluation, as WAF blocks false positives
    exclusion {
      match_variable = "RequestBodyJsonArgNames"
      operator       = "StartsWith"
      selector       = "variables."
    }

    override {
      rule_group_name = "PROTOCOL-ENFORCEMENT"
      # Missing User Agent Header, not sent from BizTalk
      rule {
        rule_id = "920320"
        enabled = true
        action  = "Log"
      }
    }

    override {
      rule_group_name = "General"
      # Failed to parse request body, XML failed to parse
      rule {
        rule_id = "200002"
        enabled = true
        action  = "Log"
      }
    }

    override {
      rule_group_name = "RCE"
      # Remote Code Execution: Unix Shell Injection, blocks GraphQL
      rule {
        rule_id = "932100"
        enabled = true
        action  = "Log"
      }
    }

    override {
      rule_group_name = "MS-ThreatIntel-SQLI"
      # SQL Comment Sequence Detected, blocks CIM/XML cim:mRID where values contains hypens
      rule {
        rule_id = "99031002"
        enabled = true
        action  = "Log"
      }
    }

    override {
      rule_group_name = "SQLI"
      # Inbound Anomaly Score Exceeded, blocks inviteUser call made through GraphQL
      rule {
        rule_id = "942190"
        enabled = true
        action  = "Log"
      }
    }

    override {
      rule_group_name = "XSS"
      # Inbound Anomaly Score Exceeded, reason: 'AngularJS client side template injection detected' - blocks requests from HoFor, see INC0400420
      rule {
        rule_id = "941380"
        enabled = true
        action  = "Log"
      }
    }
  }

  managed_rule {
    type    = "Microsoft_BotManagerRuleSet"
    version = "1.0"
    action  = "Block"
  }

  tags = local.tags
}
