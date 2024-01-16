# Register the TXT value in the DNS Zone
resource "null_resource" "add_txt_record" {
  triggers = {
    resourceId = azurerm_static_site_custom_domain.this.validation_token
  }
  provisioner "local-exec" {
    command = "az network dns record-set txt add-record --record-set-name '@' -g ${data.azurerm_resource_group.shared.name} --zone-name ${var.frontend_url} --value ${azurerm_static_site_custom_domain.this.validation_token}"
  }

  depends_on = [
    azurerm_static_site_custom_domain.this
  ]
}

resource "azurerm_dns_a_record" "this" {
  name                = "@"
  zone_name           = var.frontend_url
  resource_group_name = data.azurerm_resource_group.shared.name
  ttl                 = 3600
  target_resource_id  = azurerm_static_site.ui.id
}
