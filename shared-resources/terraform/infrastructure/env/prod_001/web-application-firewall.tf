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
      match_variable     = "RemoteAddr"
      operator           = "GeoMatch"
      negation_condition = true
      # Max 10 is allowed - US is added due to our runners
      match_values = ["NL", "DK", "US"]
    }
  }

  managed_rule {
    type    = "Microsoft_DefaultRuleSet"
    version = "2.1"
    action  = "Block"
  }

  managed_rule {
    type    = "Microsoft_BotManagerRuleSet"
    version = "1.0"
    action  = "Block"
  }
}
